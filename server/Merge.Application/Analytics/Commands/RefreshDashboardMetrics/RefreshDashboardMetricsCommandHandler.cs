using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Commands.RefreshDashboardMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class RefreshDashboardMetricsCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<RefreshDashboardMetricsCommandHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<RefreshDashboardMetricsCommand>
{

    public async Task Handle(RefreshDashboardMetricsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Refreshing dashboard metrics");
        
        var now = DateTime.UtcNow;
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var last30Days = now.AddDays(-settings.Value.DefaultPeriodDays);

        // Calculate and store metrics
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        var totalRevenue = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= last30Days)
            .SumAsync(o => o.TotalAmount, cancellationToken);

        await SaveMetricAsync("total_revenue", "Total Revenue (30d)", "Sales", totalRevenue, last30Days, now, cancellationToken);

        var totalOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .CountAsync(o => o.CreatedAt >= last30Days, cancellationToken);

        await SaveMetricAsync("total_orders", "Total Orders (30d)", "Sales", totalOrders, last30Days, now, cancellationToken);

        // Add more metrics as needed
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        logger.LogInformation("Dashboard metrics refreshed successfully");
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    private async Task SaveMetricAsync(string key, string name, string category, decimal value, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var metric = DashboardMetric.Create(key, name, category, value, start, end);

        await context.Set<DashboardMetric>().AddAsync(metric, cancellationToken);
    }
}

