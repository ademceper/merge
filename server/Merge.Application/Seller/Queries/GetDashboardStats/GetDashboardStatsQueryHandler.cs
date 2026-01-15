using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetDashboardStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, SellerDashboardStatsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetDashboardStatsQueryHandler> _logger;
    private readonly SellerSettings _sellerSettings;

    public GetDashboardStatsQueryHandler(
        IDbContext context,
        ILogger<GetDashboardStatsQueryHandler> logger,
        IOptions<SellerSettings> sellerSettings)
    {
        _context = context;
        _logger = logger;
        _sellerSettings = sellerSettings.Value;
    }

    public async Task<SellerDashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting dashboard stats. SellerId: {SellerId}", request.SellerId);

        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var sellerProfile = await _context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == request.SellerId, cancellationToken);

        if (sellerProfile == null)
        {
            _logger.LogWarning("Seller profile not found. SellerId: {SellerId}", request.SellerId);
            throw new NotFoundException("Satıcı profili", request.SellerId);
        }

        var today = DateTime.UtcNow.Date;
        
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var stats = new SellerDashboardStatsDto
        {
            TotalProducts = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .CountAsync(p => p.SellerId == request.SellerId, cancellationToken),
            ActiveProducts = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .CountAsync(p => p.SellerId == request.SellerId && p.IsActive, cancellationToken),
            TotalOrders = await _context.Set<OrderEntity>()
                .AsNoTracking()
                .CountAsync(o => o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId), cancellationToken),
            PendingOrders = await _context.Set<OrderEntity>()
                .AsNoTracking()
                .CountAsync(o => o.Status == OrderStatus.Pending &&
                           o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId), cancellationToken),
            TotalRevenue = await (
                from o in _context.Set<OrderEntity>().AsNoTracking()
                join oi in _context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
                join p in _context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
                where o.PaymentStatus == PaymentStatus.Completed && p.SellerId == request.SellerId
                select oi.TotalPrice
            ).SumAsync(cancellationToken),
            PendingBalance = sellerProfile.PendingBalance,
            AvailableBalance = sellerProfile.AvailableBalance,
            AverageRating = sellerProfile.AverageRating,
            TotalReviews = await _context.Set<ReviewEntity>()
                .AsNoTracking()
                .CountAsync(r => r.IsApproved &&
                           r.Product.SellerId == request.SellerId, cancellationToken),
            TodayOrders = await _context.Set<OrderEntity>()
                .AsNoTracking()
                .CountAsync(o => o.CreatedAt.Date == today &&
                           o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId), cancellationToken),
            TodayRevenue = await (
                from o in _context.Set<OrderEntity>().AsNoTracking()
                join oi in _context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
                join p in _context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
                where o.PaymentStatus == PaymentStatus.Completed && 
                      o.CreatedAt.Date == today && 
                      p.SellerId == request.SellerId
                select oi.TotalPrice
            ).SumAsync(cancellationToken),
            LowStockProducts = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .CountAsync(p => p.SellerId == request.SellerId &&
                           p.StockQuantity <= _sellerSettings.LowStockThreshold && p.IsActive, cancellationToken)
        };

        return stats;
    }
}
