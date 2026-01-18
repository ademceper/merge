using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler(
    IDbContext context,
    ILogger<GetDashboardStatsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching dashboard statistics");

        var stats = new DashboardStatsDto(
            TotalUsers: await context.Users.AsNoTracking().CountAsync(cancellationToken),
            ActiveUsers: await context.Users.AsNoTracking().CountAsync(u => u.EmailConfirmed, cancellationToken),
            TotalProducts: await context.Set<ProductEntity>().AsNoTracking().CountAsync(cancellationToken),
            ActiveProducts: await context.Set<ProductEntity>().AsNoTracking().CountAsync(p => p.IsActive, cancellationToken),
            TotalOrders: await context.Set<OrderEntity>().AsNoTracking().CountAsync(cancellationToken),
            TotalRevenue: await context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            PendingOrders: await context.Set<OrderEntity>().AsNoTracking().CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken),
            TodayOrders: await context.Set<OrderEntity>().AsNoTracking().CountAsync(o => o.CreatedAt.Date == DateTime.UtcNow.Date, cancellationToken),
            TodayRevenue: await context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.CreatedAt.Date == DateTime.UtcNow.Date)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            TotalWarehouses: await context.Set<Warehouse>().AsNoTracking().CountAsync(cancellationToken),
            ActiveWarehouses: await context.Set<Warehouse>().AsNoTracking().CountAsync(w => w.IsActive, cancellationToken),
            LowStockProducts: await context.Set<ProductEntity>().AsNoTracking().CountAsync(p => p.StockQuantity <= settings.Value.LowStockThreshold, cancellationToken),
            TotalCategories: await context.Set<Category>().AsNoTracking().CountAsync(cancellationToken),
            PendingReviews: await context.Set<ReviewEntity>().AsNoTracking().CountAsync(r => !r.IsApproved, cancellationToken),
            PendingReturns: await context.Set<ReturnRequest>().AsNoTracking().CountAsync(r => r.Status == ReturnRequestStatus.Pending, cancellationToken),
            Users2FAEnabled: await context.Set<TwoFactorAuth>().AsNoTracking().CountAsync(t => t.IsEnabled, cancellationToken)
        );

        logger.LogInformation("Dashboard statistics fetched successfully");
        return stats;
    }
}

