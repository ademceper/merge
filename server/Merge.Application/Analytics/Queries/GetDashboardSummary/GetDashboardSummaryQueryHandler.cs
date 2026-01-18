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
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetDashboardSummary;

public class GetDashboardSummaryQueryHandler(
    IDbContext context,
    ILogger<GetDashboardSummaryQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching dashboard summary. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);
        
        var end = request.EndDate ?? DateTime.UtcNow;
        var start = request.StartDate ?? end.AddDays(-settings.Value.DefaultDashboardPeriodDays);
        var previousStart = start.AddDays(-(end - start).Days);
        var previousEnd = start;

        var ordersQuery = context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= start && o.CreatedAt <= end);

        var previousOrdersQuery = context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= previousStart && o.CreatedAt < previousEnd);

        var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        var previousRevenue = await previousOrdersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        var revenueChange = previousRevenue > 0 ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 : 0;

        var totalOrders = await ordersQuery.CountAsync(cancellationToken);
        var previousOrderCount = await previousOrdersQuery.CountAsync(cancellationToken);
        var ordersChange = previousOrderCount > 0 ? ((decimal)(totalOrders - previousOrderCount) / previousOrderCount) * 100 : 0;

        var totalCustomers = await context.Users
            .AsNoTracking()
            .CountAsync(u => u.CreatedAt >= start && u.CreatedAt <= end, cancellationToken);

        var previousCustomers = await context.Users
            .AsNoTracking()
            .CountAsync(u => u.CreatedAt >= previousStart && u.CreatedAt < previousEnd, cancellationToken);

        var customersChange = previousCustomers > 0 ? ((decimal)(totalCustomers - previousCustomers) / previousCustomers) * 100 : 0;

        var aov = totalOrders > 0 ? totalRevenue / totalOrders : 0;
        var previousAOV = previousOrderCount > 0 ? previousRevenue / previousOrderCount : 0;
        var aovChange = previousAOV > 0 ? ((aov - previousAOV) / previousAOV) * 100 : 0;

        var pendingOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);

        var lowStockProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StockQuantity < settings.Value.LowStockThreshold, cancellationToken);

        logger.LogInformation("Dashboard summary calculated. TotalRevenue: {TotalRevenue}, TotalOrders: {TotalOrders}, TotalCustomers: {TotalCustomers}",
            totalRevenue, totalOrders, totalCustomers);

        return new DashboardSummaryDto(
            totalRevenue,
            revenueChange,
            totalOrders,
            ordersChange,
            totalCustomers,
            customersChange,
            aov,
            aovChange,
            pendingOrders,
            lowStockProducts,
            new List<DashboardMetricDto>()); // Metrics listesi şimdilik boş
    }
}

