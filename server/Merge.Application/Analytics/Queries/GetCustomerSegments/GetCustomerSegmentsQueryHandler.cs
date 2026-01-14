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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCustomerSegmentsQueryHandler : IRequestHandler<GetCustomerSegmentsQuery, List<CustomerSegmentDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetCustomerSegmentsQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;

    public GetCustomerSegmentsQueryHandler(
        IDbContext context,
        ILogger<GetCustomerSegmentsQueryHandler> logger,
        IOptions<AnalyticsSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<List<CustomerSegmentDto>> Handle(GetCustomerSegmentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching customer segments");

        // ✅ PERFORMANCE: Database'de customer segmentation yap (memory'de değil)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var vipThreshold = _settings.VipCustomerThreshold ?? 10000m; // Default VIP threshold
        var activeDaysThreshold = _settings.ActiveCustomerDaysThreshold ?? 90; // Son 90 gün içinde aktif
        var newCustomerDays = _settings.NewCustomerDaysThreshold ?? 30; // Son 30 gün içinde kayıt olanlar

        var now = DateTime.UtcNow;
        var activeDateThreshold = now.AddDays(-activeDaysThreshold);
        var newCustomerDateThreshold = now.AddDays(-newCustomerDays);

        // VIP Customers - Toplam harcaması threshold'dan fazla olanlar
        var vipCustomers = await _context.Set<OrderEntity>()
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
            ? _context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => vipCustomers.Contains(o.UserId))
            : _context.Set<OrderEntity>().AsNoTracking().Where(o => false); // Empty query

        var vipRevenue = vipCount > 0
            ? await vipOrdersQuery.SumAsync(o => o.TotalAmount, cancellationToken)
            : 0m;
        var vipOrderCount = vipCount > 0
            ? await vipOrdersQuery.CountAsync(cancellationToken)
            : 0;
        var vipAvgOrderValue = vipOrderCount > 0 ? vipRevenue / vipOrderCount : 0m;

        // Active Customers - Son X gün içinde sipariş verenler
        var activeCustomers = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= activeDateThreshold)
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var activeCount = activeCustomers.Count;
        var activeOrdersQuery = activeCount > 0
            ? _context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => activeCustomers.Contains(o.UserId) && o.CreatedAt >= activeDateThreshold)
            : _context.Set<OrderEntity>().AsNoTracking().Where(o => false); // Empty query

        var activeRevenue = activeCount > 0
            ? await activeOrdersQuery.SumAsync(o => o.TotalAmount, cancellationToken)
            : 0m;
        var activeOrderCount = activeCount > 0
            ? await activeOrdersQuery.CountAsync(cancellationToken)
            : 0;
        var activeAvgOrderValue = activeOrderCount > 0 ? activeRevenue / activeOrderCount : 0m;

        // New Customers - Son X gün içinde kayıt olanlar
        var newCustomers = await _context.Users
            .AsNoTracking()
            .Where(u => u.CreatedAt >= newCustomerDateThreshold)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var newCount = newCustomers.Count;
        var newOrdersQuery = newCount > 0
            ? _context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => newCustomers.Contains(o.UserId))
            : _context.Set<OrderEntity>().AsNoTracking().Where(o => false); // Empty query

        var newRevenue = newCount > 0
            ? await newOrdersQuery.SumAsync(o => o.TotalAmount, cancellationToken)
            : 0m;
        var newOrderCount = newCount > 0
            ? await newOrdersQuery.CountAsync(cancellationToken)
            : 0;
        var newAvgOrderValue = newOrderCount > 0 ? newRevenue / newOrderCount : 0m;

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        var segments = new List<CustomerSegmentDto>
        {
            new CustomerSegmentDto("VIP", vipCount, vipRevenue, vipAvgOrderValue),
            new CustomerSegmentDto("Active", activeCount, activeRevenue, activeAvgOrderValue),
            new CustomerSegmentDto("New", newCount, newRevenue, newAvgOrderValue)
        };

        _logger.LogInformation("Customer segments calculated. VIP: {VipCount}, Active: {ActiveCount}, New: {NewCount}",
            vipCount, activeCount, newCount);

        return segments;
    }
}

