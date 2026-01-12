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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetDashboardStatsQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;

    public GetDashboardStatsQueryHandler(
        IDbContext context,
        ILogger<GetDashboardStatsQueryHandler> logger,
        IOptions<AnalyticsSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching dashboard statistics");

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !IsDeleted checks (Global Query Filter handles it)
        var stats = new DashboardStatsDto(
            TotalUsers: await _context.Users.AsNoTracking().CountAsync(cancellationToken),
            ActiveUsers: await _context.Users.AsNoTracking().CountAsync(u => u.EmailConfirmed, cancellationToken),
            TotalProducts: await _context.Set<ProductEntity>().AsNoTracking().CountAsync(cancellationToken),
            ActiveProducts: await _context.Set<ProductEntity>().AsNoTracking().CountAsync(p => p.IsActive, cancellationToken),
            TotalOrders: await _context.Set<OrderEntity>().AsNoTracking().CountAsync(cancellationToken),
            TotalRevenue: await _context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            PendingOrders: await _context.Set<OrderEntity>().AsNoTracking().CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken),
            TodayOrders: await _context.Set<OrderEntity>().AsNoTracking().CountAsync(o => o.CreatedAt.Date == DateTime.UtcNow.Date, cancellationToken),
            TodayRevenue: await _context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.CreatedAt.Date == DateTime.UtcNow.Date)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            TotalWarehouses: await _context.Set<Warehouse>().AsNoTracking().CountAsync(cancellationToken),
            ActiveWarehouses: await _context.Set<Warehouse>().AsNoTracking().CountAsync(w => w.IsActive, cancellationToken),
            LowStockProducts: await _context.Set<ProductEntity>().AsNoTracking().CountAsync(p => p.StockQuantity <= _settings.LowStockThreshold, cancellationToken),
            TotalCategories: await _context.Set<Category>().AsNoTracking().CountAsync(cancellationToken),
            PendingReviews: await _context.Set<ReviewEntity>().AsNoTracking().CountAsync(r => !r.IsApproved, cancellationToken),
            PendingReturns: await _context.Set<ReturnRequest>().AsNoTracking().CountAsync(r => r.Status == ReturnRequestStatus.Pending, cancellationToken),
            Users2FAEnabled: await _context.Set<TwoFactorAuth>().AsNoTracking().CountAsync(t => t.IsEnabled, cancellationToken)
        );

        _logger.LogInformation("Dashboard statistics fetched successfully");
        return stats;
    }
}

