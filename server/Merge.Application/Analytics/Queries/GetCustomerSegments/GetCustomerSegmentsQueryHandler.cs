using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Analytics.Queries.GetCustomerSegments;

public class GetCustomerSegmentsQueryHandler(
    IDbContext context,
    ILogger<GetCustomerSegmentsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetCustomerSegmentsQuery, List<CustomerSegmentDto>>
{

    public async Task<List<CustomerSegmentDto>> Handle(GetCustomerSegmentsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching customer segments");

        
        var vipThreshold = settings.Value.VipCustomerThreshold ?? 10000m; // Default VIP threshold
        var activeDaysThreshold = settings.Value.ActiveCustomerDaysThreshold ?? 90; // Son 90 gün içinde aktif
        var newCustomerDays = settings.Value.NewCustomerDaysThreshold ?? 30; // Son 30 gün içinde kayıt olanlar

        var now = DateTime.UtcNow;
        var activeDateThreshold = now.AddDays(-activeDaysThreshold);
        var newCustomerDateThreshold = now.AddDays(-newCustomerDays);

        // VIP Customers - Toplam harcaması threshold'dan fazla olanlar
        var vipCustomers = await context.Set<OrderEntity>()
            .AsNoTracking()
            .GroupBy(o => o.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalRevenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .Where(x => x.TotalRevenue >= vipThreshold)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);

        var vipCount = vipCustomers.Count;
        var vipOrdersQuery = vipCount > 0
            ? context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => vipCustomers.Contains(o.UserId))
            : context.Set<OrderEntity>().AsNoTracking().Where(o => false); // Empty query

        var vipRevenue = vipCount > 0
            ? await vipOrdersQuery.SumAsync(o => o.TotalAmount, cancellationToken)
            : 0m;
        var vipOrderCount = vipCount > 0
            ? await vipOrdersQuery.CountAsync(cancellationToken)
            : 0;
        var vipAvgOrderValue = vipOrderCount > 0 ? vipRevenue / vipOrderCount : 0m;

        // Active Customers - Son X gün içinde sipariş verenler
        var activeCustomers = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= activeDateThreshold)
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var activeCount = activeCustomers.Count;
        var activeOrdersQuery = activeCount > 0
            ? context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => activeCustomers.Contains(o.UserId) && o.CreatedAt >= activeDateThreshold)
            : context.Set<OrderEntity>().AsNoTracking().Where(o => false); // Empty query

        var activeRevenue = activeCount > 0
            ? await activeOrdersQuery.SumAsync(o => o.TotalAmount, cancellationToken)
            : 0m;
        var activeOrderCount = activeCount > 0
            ? await activeOrdersQuery.CountAsync(cancellationToken)
            : 0;
        var activeAvgOrderValue = activeOrderCount > 0 ? activeRevenue / activeOrderCount : 0m;

        // New Customers - Son X gün içinde kayıt olanlar
        var newCustomers = await context.Users
            .AsNoTracking()
            .Where(u => u.CreatedAt >= newCustomerDateThreshold)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var newCount = newCustomers.Count;
        var newOrdersQuery = newCount > 0
            ? context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => newCustomers.Contains(o.UserId))
            : context.Set<OrderEntity>().AsNoTracking().Where(o => false); // Empty query

        var newRevenue = newCount > 0
            ? await newOrdersQuery.SumAsync(o => o.TotalAmount, cancellationToken)
            : 0m;
        var newOrderCount = newCount > 0
            ? await newOrdersQuery.CountAsync(cancellationToken)
            : 0;
        var newAvgOrderValue = newOrderCount > 0 ? newRevenue / newOrderCount : 0m;

        var segments = new List<CustomerSegmentDto>
        {
            new CustomerSegmentDto("VIP", vipCount, vipRevenue, vipAvgOrderValue),
            new CustomerSegmentDto("Active", activeCount, activeRevenue, activeAvgOrderValue),
            new CustomerSegmentDto("New", newCount, newRevenue, newAvgOrderValue)
        };

        logger.LogInformation("Customer segments calculated. VIP: {VipCount}, Active: {ActiveCount}, New: {NewCount}",
            vipCount, activeCount, newCount);

        return segments;
    }
}

