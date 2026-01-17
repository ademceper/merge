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
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetStoreStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetStoreStatsQueryHandler(IDbContext context, ILogger<GetStoreStatsQueryHandler> logger, IOptions<ServiceSettings> serviceSettings) : IRequestHandler<GetStoreStatsQuery, StoreStatsDto>
{
    private readonly ServiceSettings serviceConfig = serviceSettings.Value;

    public async Task<StoreStatsDto> Handle(GetStoreStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Getting store stats. StoreId: {StoreId}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.StoreId, request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted (Global Query Filter)
        var store = await context.Set<Store>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.StoreId, cancellationToken);

        if (store == null)
        {
            logger.LogWarning("Store not found. StoreId: {StoreId}", request.StoreId);
            throw new NotFoundException("Mağaza", request.StoreId);
        }

        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-serviceConfig.DefaultDateRangeDays); // ✅ BOLUM 12.0: Magic number config'den
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var totalProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == request.StoreId, cancellationToken);

        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var activeProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StoreId.HasValue && p.StoreId.Value == request.StoreId && p.IsActive, cancellationToken);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == request.StoreId))
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Explicit Join yaklaşımı - tek sorgu (N+1 fix)
        var totalRevenue = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  p.StoreId.HasValue && p.StoreId.Value == request.StoreId
            select oi.TotalPrice
        ).SumAsync(cancellationToken);

        // ✅ PERFORMANCE: Explicit Join yaklaşımı - tek sorgu (N+1 fix)
        var monthlyRevenue = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  p.StoreId.HasValue && p.StoreId.Value == request.StoreId
            select oi.TotalPrice
        ).SumAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de distinct count yap (memory'de işlem YASAK)
        var totalCustomers = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.OrderItems.Any(oi => oi.Product != null && oi.Product.StoreId == request.StoreId))
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de average yap (memory'de işlem YASAK)
        var averageRating = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.IsApproved &&
                  r.Product != null && r.Product.StoreId.HasValue && r.Product.StoreId.Value == request.StoreId)
            .AverageAsync(r => (double?)r.Rating, cancellationToken) ?? 0;

        return new StoreStatsDto
        {
            StoreId = request.StoreId,
            StoreName = store.StoreName,
            TotalProducts = totalProducts,
            ActiveProducts = activeProducts,
            TotalOrders = totalOrders,
            TotalRevenue = Math.Round(totalRevenue, 2),
            MonthlyRevenue = Math.Round(monthlyRevenue, 2),
            TotalCustomers = totalCustomers,
            AverageRating = Math.Round((decimal)averageRating, 2)
        };
    }
}
