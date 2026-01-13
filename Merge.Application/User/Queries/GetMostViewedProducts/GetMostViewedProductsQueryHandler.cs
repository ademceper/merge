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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetMostViewedProductsQueryHandler : IRequestHandler<GetMostViewedProductsQuery, List<PopularProductDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetMostViewedProductsQueryHandler> _logger;
    private readonly UserSettings _userSettings;

    public GetMostViewedProductsQueryHandler(
        IDbContext context,
        ILogger<GetMostViewedProductsQueryHandler> logger,
        IOptions<UserSettings> userSettings)
    {
        _context = context;
        _logger = logger;
        _userSettings = userSettings.Value;
    }

    public async Task<List<PopularProductDto>> Handle(GetMostViewedProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving most viewed products for last {Days} days, top {TopN}", request.Days, request.TopN);

        // ✅ BOLUM 12.0: Magic numbers configuration'dan alınıyor
        var days = request.Days;
        if (days > _userSettings.Activity.MaxDays) days = _userSettings.Activity.MaxDays;
        if (days < 1) days = _userSettings.Activity.DefaultDays;

        var topN = request.TopN;
        if (topN > _userSettings.Activity.MaxTopN) topN = _userSettings.Activity.MaxTopN;
        if (topN < 1) topN = _userSettings.Activity.DefaultTopN;

        var startDate = DateTime.UtcNow.AddDays(-days);

        var productIds = await _context.Set<UserActivityLog>()
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

        var productActivitiesData = await _context.Set<UserActivityLog>()
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

        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        var purchases = await _context.Set<OrderItem>()
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
