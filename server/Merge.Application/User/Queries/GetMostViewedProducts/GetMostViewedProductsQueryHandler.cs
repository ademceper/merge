using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.User.Queries.GetMostViewedProducts;

public class GetMostViewedProductsQueryHandler(IDbContext context, ILogger<GetMostViewedProductsQueryHandler> logger, IOptions<UserSettings> userSettings) : IRequestHandler<GetMostViewedProductsQuery, List<PopularProductDto>>
{
    public async Task<List<PopularProductDto>> Handle(GetMostViewedProductsQuery request, CancellationToken cancellationToken)
    {

        logger.LogInformation("Retrieving most viewed products for last {Days} days, top {TopN}", request.Days, request.TopN);
        var days = request.Days;
        if (days > userSettings.Value.Activity.MaxDays) days = userSettings.Value.Activity.MaxDays;
        if (days < 1) days = userSettings.Value.Activity.DefaultDays;

        var topN = request.TopN;
        if (topN > userSettings.Value.Activity.MaxTopN) topN = userSettings.Value.Activity.MaxTopN;
        if (topN < 1) topN = userSettings.Value.Activity.DefaultTopN;

        var startDate = DateTime.UtcNow.AddDays(-days);

        var productIds =         // ✅ PERFORMANCE: AsNoTracking
        await context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate &&
                       a.EntityType == EntityType.Product &&
                       a.EntityId.HasValue &&
                       (a.ActivityType == ActivityType.ViewProduct ||
                        a.ActivityType == ActivityType.AddToCart))
            .GroupBy(a => a.EntityId)
            .Select(g => new
            {
                ProductId = g.Key!.Value,
                ViewCount = g.Count(a => a.ActivityType == ActivityType.ViewProduct)
            })
            .OrderByDescending(p => p.ViewCount)
            .Take(topN)
            .Select(p => p.ProductId)
            .ToListAsync(cancellationToken);

        var productActivitiesData =         // ✅ PERFORMANCE: AsNoTracking
        await context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate &&
                       a.EntityType == EntityType.Product &&
                       a.EntityId.HasValue &&
                       productIds.Contains(a.EntityId.Value) &&
                       (a.ActivityType == ActivityType.ViewProduct ||
                        a.ActivityType == ActivityType.AddToCart))
            .GroupBy(a => a.EntityId)
            .Select(g => new
            {
                ProductId = g.Key!.Value,
                ViewCount = g.Count(a => a.ActivityType == ActivityType.ViewProduct),
                AddToCartCount = g.Count(a => a.ActivityType == ActivityType.AddToCart)
            })
            .OrderByDescending(p => p.ViewCount)
            .Take(topN)
            .ToListAsync(cancellationToken);

        var products =         // ✅ PERFORMANCE: AsNoTracking
        await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        var purchases =         // ✅ PERFORMANCE: AsNoTracking
        await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => productIds.Contains(oi.ProductId) &&
                        oi.Order.CreatedAt >= startDate)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, PurchaseCount = g.Sum(oi => oi.Quantity) })
            .ToDictionaryAsync(p => p.ProductId, p => p.PurchaseCount, cancellationToken);

        var result = new List<PopularProductDto>(productActivitiesData.Count);
        foreach (var p in productActivitiesData)
        {
            var purchaseCount = purchases.ContainsKey(p.ProductId) ? purchases[p.ProductId] : 0;
            var conversionRate = p.ViewCount > 0
                ? (decimal)purchaseCount / p.ViewCount * 100
                : 0;
            
            result.Add(new PopularProductDto
            {
                ProductId = p.ProductId,
                ProductName = products.ContainsKey(p.ProductId) ? products[p.ProductId] : "Unknown",
                ViewCount = p.ViewCount,
                AddToCartCount = p.AddToCartCount,
                PurchaseCount = purchaseCount,
                ConversionRate = conversionRate
            });
        }
        return result;
    }
}
