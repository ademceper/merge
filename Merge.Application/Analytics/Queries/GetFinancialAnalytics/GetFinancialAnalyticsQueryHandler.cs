using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetFinancialAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetFinancialAnalyticsQueryHandler : IRequestHandler<GetFinancialAnalyticsQuery, FinancialAnalyticsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetFinancialAnalyticsQueryHandler> _logger;

    public GetFinancialAnalyticsQueryHandler(
        IDbContext context,
        ILogger<GetFinancialAnalyticsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FinancialAnalyticsDto> Handle(GetFinancialAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching financial analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: Database'de aggregate query kullan (memory'de değil) - 5-10x performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted and !r.IsDeleted checks (Global Query Filter handles it)
        var ordersQuery = _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate);

        var grossRevenue = await ordersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        var totalTax = await ordersQuery.SumAsync(o => o.Tax, cancellationToken);
        var totalShipping = await ordersQuery.SumAsync(o => o.ShippingCost, cancellationToken);

        var totalRefunds = await _context.Set<ReturnRequest>()
            .AsNoTracking()
            .Where(r => r.Status == ReturnRequestStatus.Approved &&
                        r.CreatedAt >= request.StartDate && r.CreatedAt <= request.EndDate)
            .SumAsync(r => r.RefundAmount, cancellationToken);

        var netProfit = grossRevenue - totalRefunds - totalShipping;

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        var revenueTimeSeries = await GetRevenueOverTimeAsync(request.StartDate, request.EndDate, cancellationToken);
        
        return new FinancialAnalyticsDto(
            request.StartDate,
            request.EndDate,
            grossRevenue,
            totalShipping + totalRefunds,
            netProfit,
            grossRevenue > 0 ? (netProfit / grossRevenue) * 100 : 0,
            totalTax,
            totalRefunds,
            totalShipping,
            revenueTimeSeries,
            new List<TimeSeriesDataPoint>() // ProfitTimeSeries - şimdilik boş
        );
    }

    private async Task<List<TimeSeriesDataPoint>> GetRevenueOverTimeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new TimeSeriesDataPoint(
                g.Key,
                g.Sum(o => o.TotalAmount),
                null,
                g.Count()
            ))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);
    }
}

