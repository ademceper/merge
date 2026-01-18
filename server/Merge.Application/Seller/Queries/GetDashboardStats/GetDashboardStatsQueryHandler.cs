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

public class GetDashboardStatsQueryHandler(IDbContext context, ILogger<GetDashboardStatsQueryHandler> logger, IOptions<SellerSettings> sellerSettings) : IRequestHandler<GetDashboardStatsQuery, SellerDashboardStatsDto>
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;

    public async Task<SellerDashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting dashboard stats. SellerId: {SellerId}", request.SellerId);

        var sellerProfile = await context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == request.SellerId, cancellationToken);

        if (sellerProfile == null)
        {
            logger.LogWarning("Seller profile not found. SellerId: {SellerId}", request.SellerId);
            throw new NotFoundException("Satıcı profili", request.SellerId);
        }

        var today = DateTime.UtcNow.Date;
        
        var stats = new SellerDashboardStatsDto
        {
            TotalProducts = await context.Set<ProductEntity>()
                .AsNoTracking()
                .CountAsync(p => p.SellerId == request.SellerId, cancellationToken),
            ActiveProducts = await context.Set<ProductEntity>()
                .AsNoTracking()
                .CountAsync(p => p.SellerId == request.SellerId && p.IsActive, cancellationToken),
            TotalOrders = await context.Set<OrderEntity>()
                .AsNoTracking()
                .CountAsync(o => o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId), cancellationToken),
            PendingOrders = await context.Set<OrderEntity>()
                .AsNoTracking()
                .CountAsync(o => o.Status == OrderStatus.Pending &&
                           o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId), cancellationToken),
            TotalRevenue = await (
                from o in context.Set<OrderEntity>().AsNoTracking()
                join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
                join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
                where o.PaymentStatus == PaymentStatus.Completed && p.SellerId == request.SellerId
                select oi.TotalPrice
            ).SumAsync(cancellationToken),
            PendingBalance = sellerProfile.PendingBalance,
            AvailableBalance = sellerProfile.AvailableBalance,
            AverageRating = sellerProfile.AverageRating,
            TotalReviews = await context.Set<ReviewEntity>()
                .AsNoTracking()
                .CountAsync(r => r.IsApproved &&
                           r.Product.SellerId == request.SellerId, cancellationToken),
            TodayOrders = await context.Set<OrderEntity>()
                .AsNoTracking()
                .CountAsync(o => o.CreatedAt.Date == today &&
                           o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId), cancellationToken),
            TodayRevenue = await (
                from o in context.Set<OrderEntity>().AsNoTracking()
                join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
                join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
                where o.PaymentStatus == PaymentStatus.Completed && 
                      o.CreatedAt.Date == today && 
                      p.SellerId == request.SellerId
                select oi.TotalPrice
            ).SumAsync(cancellationToken),
            LowStockProducts = await context.Set<ProductEntity>()
                .AsNoTracking()
                .CountAsync(p => p.SellerId == request.SellerId &&
                           p.StockQuantity <= sellerConfig.LowStockThreshold && p.IsActive, cancellationToken)
        };

        return stats;
    }
}
