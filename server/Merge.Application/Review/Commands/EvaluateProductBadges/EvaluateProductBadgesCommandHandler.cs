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

public class EvaluateProductBadgesCommandHandler(IDbContext context, IMediator mediator, ILogger<EvaluateProductBadgesCommandHandler> logger) : IRequestHandler<EvaluateProductBadgesCommand>
{

    public async Task Handle(EvaluateProductBadgesCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Evaluating product badges. ProductId: {ProductId}", request.ProductId);

        var badges = await context.Set<TrustBadge>()
            .AsNoTracking()
            .Where(b => b.IsActive && b.BadgeType == "Product")
            .ToListAsync(cancellationToken);

        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null) return;

        // Get product metrics
        var totalSales = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.ProductId == request.ProductId && oi.Order.PaymentStatus == PaymentStatus.Completed)
            .SumAsync(oi => oi.Quantity, cancellationToken);

        var averageRating = product.Rating;
        var totalReviews = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .CountAsync(r => r.ProductId == request.ProductId && r.IsApproved, cancellationToken);

        var daysSinceCreated = (DateTime.UtcNow - product.CreatedAt).Days;

        foreach (var badge in badges)
        {
            if (string.IsNullOrEmpty(badge.Criteria)) continue;

            var criteria = JsonSerializer.Deserialize<Dictionary<string, object>>(badge.Criteria);
            if (criteria is null) continue;

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
                var existing = await context.Set<ProductTrustBadge>()
                    .FirstOrDefaultAsync(ptb => ptb.ProductId == request.ProductId && ptb.TrustBadgeId == badge.Id, cancellationToken);

                if (existing is null)
                {
                    var awardCommand = new AwardProductBadgeCommand(
                        request.ProductId,
                        badge.Id,
                        null,
                        "Automatically awarded based on product performance");
                    await mediator.Send(awardCommand, cancellationToken);
                }
            }
        }

        logger.LogInformation("Product badges evaluated successfully. ProductId: {ProductId}", request.ProductId);
    }
}
