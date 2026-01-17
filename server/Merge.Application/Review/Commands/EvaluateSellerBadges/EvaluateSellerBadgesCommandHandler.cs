using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Review.Commands.AwardSellerBadge;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.EvaluateSellerBadges;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class EvaluateSellerBadgesCommandHandler(IDbContext context, IMediator mediator, ILogger<EvaluateSellerBadgesCommandHandler> logger) : IRequestHandler<EvaluateSellerBadgesCommand>
{

    public async Task Handle(EvaluateSellerBadgesCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Evaluating seller badges. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !b.IsDeleted (Global Query Filter)
        var badges = await context.Set<TrustBadge>()
            .AsNoTracking()
            .Where(b => b.IsActive && b.BadgeType == "Seller")
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var seller = await context.Set<SellerProfile>()
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == request.SellerId, cancellationToken);

        if (seller == null) return;

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted, !r.IsDeleted (Global Query Filter)
        // Get seller metrics
        var totalOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .CountAsync(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId), cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalRevenue = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => oi.Product.SellerId == request.SellerId &&
                  oi.Order.PaymentStatus == PaymentStatus.Completed)
            .SumAsync(oi => oi.TotalPrice, cancellationToken);

        var averageRating = seller.AverageRating;
        var totalReviews = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .CountAsync(r => r.IsApproved &&
                  r.Product.SellerId == request.SellerId, cancellationToken);

        var daysSinceJoined = (DateTime.UtcNow - seller.CreatedAt).Days;

        foreach (var badge in badges)
        {
            if (string.IsNullOrEmpty(badge.Criteria)) continue;

            var criteria = JsonSerializer.Deserialize<Dictionary<string, object>>(badge.Criteria);
            if (criteria == null) continue;

            bool qualifies = true;

            // Check criteria
            if (criteria.ContainsKey("minOrders") && totalOrders < Convert.ToInt32(criteria["minOrders"]))
                qualifies = false;
            if (criteria.ContainsKey("minRevenue") && totalRevenue < Convert.ToDecimal(criteria["minRevenue"]))
                qualifies = false;
            if (criteria.ContainsKey("minRating") && averageRating < Convert.ToDecimal(criteria["minRating"]))
                qualifies = false;
            if (criteria.ContainsKey("minReviews") && totalReviews < Convert.ToInt32(criteria["minReviews"]))
                qualifies = false;
            if (criteria.ContainsKey("minDaysActive") && daysSinceJoined < Convert.ToInt32(criteria["minDaysActive"]))
                qualifies = false;

            if (qualifies)
            {
                // ✅ PERFORMANCE: Removed manual !stb.IsDeleted (Global Query Filter)
                var existing = await context.Set<SellerTrustBadge>()
                    .FirstOrDefaultAsync(stb => stb.SellerId == request.SellerId && stb.TrustBadgeId == badge.Id, cancellationToken);

                if (existing == null)
                {
                    // ✅ BOLUM 2.0: MediatR + CQRS pattern - Command çağrısı
                    var awardCommand = new AwardSellerBadgeCommand(
                        request.SellerId,
                        badge.Id,
                        null,
                        "Automatically awarded based on performance criteria");
                    await mediator.Send(awardCommand, cancellationToken);
                }
            }
        }

        logger.LogInformation("Seller badges evaluated successfully. SellerId: {SellerId}", request.SellerId);
    }
}
