using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Review.Commands.AwardProductBadge;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.EvaluateProductBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class EvaluateProductBadgesCommandHandler : IRequestHandler<EvaluateProductBadgesCommand>
{
    private readonly IDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<EvaluateProductBadgesCommandHandler> _logger;

    public EvaluateProductBadgesCommandHandler(
        IDbContext context,
        IMediator mediator,
        ILogger<EvaluateProductBadgesCommandHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(EvaluateProductBadgesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Evaluating product badges. ProductId: {ProductId}", request.ProductId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var badges = await _context.Set<TrustBadge>()
            .AsNoTracking()
            .Where(b => b.IsActive && b.BadgeType == "Product")
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var product = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null) return;

        // ✅ PERFORMANCE: Removed manual !oi.Order.IsDeleted, !r.IsDeleted (Global Query Filter)
        // Get product metrics
        var totalSales = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.ProductId == request.ProductId && oi.Order.PaymentStatus == PaymentStatus.Completed)
            .SumAsync(oi => oi.Quantity, cancellationToken);

        var averageRating = product.Rating;
        var totalReviews = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .CountAsync(r => r.ProductId == request.ProductId && r.IsApproved, cancellationToken);

        var daysSinceCreated = (DateTime.UtcNow - product.CreatedAt).Days;

        foreach (var badge in badges)
        {
            if (string.IsNullOrEmpty(badge.Criteria)) continue;

            var criteria = JsonSerializer.Deserialize<Dictionary<string, object>>(badge.Criteria);
            if (criteria == null) continue;

            bool qualifies = true;

            // Check criteria
            if (criteria.ContainsKey("minSales") && totalSales < Convert.ToInt32(criteria["minSales"]))
                qualifies = false;
            if (criteria.ContainsKey("minRating") && averageRating < Convert.ToDecimal(criteria["minRating"]))
                qualifies = false;
            if (criteria.ContainsKey("minReviews") && totalReviews < Convert.ToInt32(criteria["minReviews"]))
                qualifies = false;
            if (criteria.ContainsKey("minDaysActive") && daysSinceCreated < Convert.ToInt32(criteria["minDaysActive"]))
                qualifies = false;
            if (criteria.ContainsKey("minStock") && product.StockQuantity < Convert.ToInt32(criteria["minStock"]))
                qualifies = false;

            if (qualifies)
            {
                // ✅ PERFORMANCE: Removed manual !ptb.IsDeleted (Global Query Filter)
                var existing = await _context.Set<ProductTrustBadge>()
                    .FirstOrDefaultAsync(ptb => ptb.ProductId == request.ProductId && ptb.TrustBadgeId == badge.Id, cancellationToken);

                if (existing == null)
                {
                    // ✅ BOLUM 2.0: MediatR + CQRS pattern - Command çağrısı
                    var awardCommand = new AwardProductBadgeCommand(
                        request.ProductId,
                        badge.Id,
                        null,
                        "Automatically awarded based on product performance");
                    await _mediator.Send(awardCommand, cancellationToken);
                }
            }
        }

        _logger.LogInformation("Product badges evaluated successfully. ProductId: {ProductId}", request.ProductId);
    }
}
