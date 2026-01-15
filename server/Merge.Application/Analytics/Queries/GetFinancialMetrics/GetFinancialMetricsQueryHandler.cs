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

namespace Merge.Application.Analytics.Queries.GetFinancialMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetFinancialMetricsQueryHandler(
    IDbContext context,
    ILogger<GetFinancialMetricsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetFinancialMetricsQuery, FinancialMetricsDto>
{

    public async Task<FinancialMetricsDto> Handle(GetFinancialMetricsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching financial metrics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-settings.Value.DefaultPeriodDays);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var ordersQuery = context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate &&
                  o.CreatedAt <= endDate);

        var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        var totalOrders = await ordersQuery.CountAsync(cancellationToken);
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var totalCosts = totalRevenue * settings.Value.DefaultCostPercentage;
        var netProfit = totalRevenue - totalCosts;

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        return new FinancialMetricsDto(
            TotalRevenue: Math.Round(totalRevenue, 2),
            TotalCosts: Math.Round(totalCosts, 2),
            NetProfit: Math.Round(netProfit, 2),
            ProfitMargin: totalRevenue > 0 ? Math.Round((netProfit / totalRevenue) * 100, 2) : 0,
            AverageOrderValue: totalOrders > 0 ? Math.Round(totalRevenue / totalOrders, 2) : 0,
            TotalOrders: totalOrders
        );
    }
}

