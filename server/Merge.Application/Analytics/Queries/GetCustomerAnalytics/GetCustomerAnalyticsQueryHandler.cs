using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetCustomerAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCustomerAnalyticsQueryHandler(
    IDbContext context,
    ILogger<GetCustomerAnalyticsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings,
    IMapper mapper) : IRequestHandler<GetCustomerAnalyticsQuery, CustomerAnalyticsDto>
{

    public async Task<CustomerAnalyticsDto> Handle(GetCustomerAnalyticsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching customer analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ Identity framework'ün Role ve UserRole entity'leri IDbContext üzerinden erişiliyor
        var customerRole = await context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == "Customer", cancellationToken);
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var customerUserIds = customerRole != null
            ? await context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.RoleId == customerRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync(cancellationToken)
            : new List<Guid>(0); // Pre-allocate with known capacity (0)
        
        // ✅ PERFORMANCE: Database'de filtreleme yap (memory'de değil)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !u.IsDeleted and !o.IsDeleted checks (Global Query Filter handles it)
        // ✅ PERFORMANCE: List.Count > 0 kullan (Any() YASAK - .cursorrules)
        var totalCustomers = customerUserIds.Count > 0
            ? await context.Users
                .AsNoTracking()
                .Where(u => customerUserIds.Contains(u.Id))
                .CountAsync(cancellationToken)
            : 0;

        var newCustomers = customerUserIds.Count > 0
            ? await context.Users
                .AsNoTracking()
                .Where(u => customerUserIds.Contains(u.Id) && u.CreatedAt >= request.StartDate && u.CreatedAt <= request.EndDate)
                .CountAsync(cancellationToken)
            : 0;

        var activeCustomers = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate)
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var topCustomers = await GetTopCustomersAsync(settings.Value.MaxQueryLimit, cancellationToken);
        var customerSegments = await GetCustomerSegmentsAsync(cancellationToken);
        
        return new CustomerAnalyticsDto(
            request.StartDate,
            request.EndDate,
            totalCustomers,
            newCustomers,
            activeCustomers,
            0, // ReturningCustomers - şimdilik 0
            0, // AverageLifetimeValue - şimdilik 0
            0, // AveragePurchaseFrequency - şimdilik 0
            customerSegments,
            topCustomers,
            new List<TimeSeriesDataPoint>() // CustomerAcquisition - şimdilik boş
        );
    }

    private async Task<List<TopCustomerDto>> GetTopCustomersAsync(int limit, CancellationToken cancellationToken)
    {
        return await context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.User)
            .GroupBy(o => new { o.UserId, o.User.FirstName, o.User.LastName, o.User.Email })
            .Select(g => new TopCustomerDto(
                g.Key.UserId,
                $"{g.Key.FirstName} {g.Key.LastName}",
                g.Key.Email ?? string.Empty,
                g.Count(),
                g.Sum(o => o.TotalAmount),
                g.Max(o => o.CreatedAt)
            ))
            .OrderByDescending(c => c.TotalSpent)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<CustomerSegmentDto>> GetCustomerSegmentsAsync(CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Database'de customer segmentation yap (memory'de değil)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var vipThreshold = settings.Value.VipCustomerThreshold ?? 10000m;
        var activeDaysThreshold = settings.Value.ActiveCustomerDaysThreshold ?? 90;
        var newCustomerDays = settings.Value.NewCustomerDaysThreshold ?? 30;

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
                TotalRevenue = g.Sum(o => o.TotalAmount)
            })
            .Where(x => x.TotalRevenue >= vipThreshold)
            .Select(x => x.UserId)
            .ToListAsync(cancellationToken);

        var vipCount = vipCustomers.Count;
        var vipOrdersQuery = vipCount > 0
            ? context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => vipCustomers.Contains(o.UserId))
            : context.Set<OrderEntity>().AsNoTracking().Where(o => false);

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
            : context.Set<OrderEntity>().AsNoTracking().Where(o => false);

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
            : context.Set<OrderEntity>().AsNoTracking().Where(o => false);

        var newRevenue = newCount > 0
            ? await newOrdersQuery.SumAsync(o => o.TotalAmount, cancellationToken)
            : 0m;
        var newOrderCount = newCount > 0
            ? await newOrdersQuery.CountAsync(cancellationToken)
            : 0;
        var newAvgOrderValue = newOrderCount > 0 ? newRevenue / newOrderCount : 0m;

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        return new List<CustomerSegmentDto>
        {
            new CustomerSegmentDto("VIP", vipCount, vipRevenue, vipAvgOrderValue),
            new CustomerSegmentDto("Active", activeCount, activeRevenue, activeAvgOrderValue),
            new CustomerSegmentDto("New", newCount, newRevenue, newAvgOrderValue)
        };
    }
}

