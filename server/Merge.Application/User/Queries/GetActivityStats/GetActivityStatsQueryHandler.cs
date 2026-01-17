using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using OrderItem = Merge.Domain.Modules.Ordering.OrderItem;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.User.Queries.GetActivityStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetActivityStatsQueryHandler(IDbContext context, ILogger<GetActivityStatsQueryHandler> logger, IOptions<UserSettings> userSettings) : IRequestHandler<GetActivityStatsQuery, ActivityStatsDto>
{
    private readonly UserSettings config = userSettings.Value;

    public async Task<ActivityStatsDto> Handle(GetActivityStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        logger.LogInformation("Generating activity statistics for last {Days} days", request.Days);
        var days = request.Days;
        if (days > config.Activity.MaxDays) days = config.Activity.MaxDays;
        if (days < 1) days = config.Activity.DefaultDays;

        var startDate = DateTime.UtcNow.AddDays(-days);

        IQueryable<UserActivityLog> query = context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate);

        var totalActivities = await query.CountAsync(cancellationToken);
        var uniqueUsers = await query
            .Where(a => a.UserId.HasValue)
            .Select(a => a.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        var activitiesByType = await query
            .GroupBy(a => a.ActivityType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type.ToString(), x => x.Count, cancellationToken);

        var activitiesByDevice = await query
            .GroupBy(a => a.DeviceType)
            .Select(g => new { Device = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Device.ToString(), x => x.Count, cancellationToken);

        var activitiesByHour = await query
            .GroupBy(a => a.CreatedAt.Hour)
            .Select(g => new { Hour = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Hour, x => x.Count, cancellationToken);

        var userIds = await query
            .Where(a => a.UserId.HasValue)
            .GroupBy(a => a.UserId)
            .Select(g => new
            {
                UserId = g.Key!.Value,
                ActivityCount = g.Count()
            })
            .OrderByDescending(u => u.ActivityCount)
            .Take(config.Activity.DefaultTopN)
            .Select(u => u.UserId)
            .ToListAsync(cancellationToken);
        var userEmails = await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email, cancellationToken);

        var topUsersData = await query
            .Where(a => a.UserId.HasValue && userIds.Contains(a.UserId.Value))
            .GroupBy(a => a.UserId)
            .Select(g => new TopUserActivityDto
            {
                UserId = g.Key!.Value,
                UserEmail = string.Empty,
                ActivityCount = g.Count(),
                LastActivity = g.Max(a => a.CreatedAt)
            })
            .OrderByDescending(u => u.ActivityCount)
            .Take(config.Activity.DefaultTopN)
            .ToListAsync(cancellationToken);

        foreach (var user in topUsersData)
        {
            if (userEmails.TryGetValue(user.UserId, out var email) && !string.IsNullOrEmpty(email))
            {
                user.UserEmail = email;
            }
        }
        var mostViewedProducts = await GetMostViewedProductsAsync(days, config.Activity.DefaultTopN, cancellationToken);

        var avgSessionDuration = await query
            .Where(a => a.DurationMs > 0)
            .AverageAsync(a => (decimal?)a.DurationMs, cancellationToken) ?? 0;

        logger.LogInformation("Activity stats generated - Total: {Total}, Unique Users: {Users}", totalActivities, uniqueUsers);

        return new ActivityStatsDto
        {
            TotalActivities = totalActivities,
            UniqueUsers = uniqueUsers,
            ActivitiesByType = activitiesByType,
            ActivitiesByDevice = activitiesByDevice,
            ActivitiesByHour = activitiesByHour,
            TopUsers = topUsersData,
            MostViewedProducts = mostViewedProducts,
            AverageSessionDuration = avgSessionDuration
        };
    }

    private async Task<List<PopularProductDto>> GetMostViewedProductsAsync(int days, int topN, CancellationToken cancellationToken)
    {
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
