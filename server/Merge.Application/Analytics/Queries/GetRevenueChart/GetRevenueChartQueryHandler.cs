using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetRevenueChart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetRevenueChartQueryHandler(
    IDbContext context,
    ILogger<GetRevenueChartQueryHandler> logger,
    IOptions<ServiceSettings> serviceSettings) : IRequestHandler<GetRevenueChartQuery, RevenueChartDto>
{

    public async Task<RevenueChartDto> Handle(GetRevenueChartQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching revenue chart. Days: {Days}", request.Days);
        
        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        var days = request.Days == 30 ? serviceSettings.Value.DefaultDateRangeDays : request.Days;
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        // ✅ PERFORMANCE: Database'de toplam hesapla (memory'de Sum YASAK)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        var ordersQuery = context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.CreatedAt >= startDate);

        var dailyRevenue = await ordersQuery
            .GroupBy(o => o.CreatedAt.Date)
            // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
            .Select(g => new DailyRevenueDto(
                g.Key,
                g.Sum(o => o.TotalAmount),
                g.Count()
            ))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de toplam hesapla (memory'de Sum yerine)
        var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        var totalOrders = await ordersQuery.CountAsync(cancellationToken);

        var chart = new RevenueChartDto(
            Days: days,
            TotalRevenue: totalRevenue,
            TotalOrders: totalOrders,
            DailyData: dailyRevenue
        );

        logger.LogInformation("Revenue chart calculated. Days: {Days}, TotalRevenue: {TotalRevenue}, TotalOrders: {TotalOrders}",
            days, totalRevenue, totalOrders);

        return chart;
    }
}

