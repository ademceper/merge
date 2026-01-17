using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetPerformanceMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPerformanceMetricsQueryHandler(IDbContext context, ILogger<GetPerformanceMetricsQueryHandler> logger, IOptions<SellerSettings> sellerSettings) : IRequestHandler<GetPerformanceMetricsQuery, SellerPerformanceDto>
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;

    public async Task<SellerPerformanceDto> Handle(GetPerformanceMetricsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Getting performance metrics. SellerId: {SellerId}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.SellerId, request.StartDate, request.EndDate);

        // ✅ BOLUM 12.0: Magic number config'den - SellerSettings kullanımı
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-sellerConfig.DefaultStatsPeriodDays);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Explicit Join yaklaşımı - tek sorgu (N+1 fix)
        var totalSales = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  p.SellerId == request.SellerId
            select oi.TotalPrice
        ).SumAsync(cancellationToken);

        var totalOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId))
            .CountAsync(cancellationToken);

        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Explicit Join yaklaşımı - tek sorgu (N+1 fix)
        // Sales by date
        var salesByDate = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  p.SellerId == request.SellerId
            group new { o, oi } by o.CreatedAt.Date into g
            select new SalesByDateDto
            {
                Date = g.Key,
                Sales = g.Sum(x => x.oi.TotalPrice),
                OrderCount = g.Select(x => x.o.Id).Distinct().Count()
            }
        ).OrderBy(s => s.Date).ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // Top products
        var topProducts = await context.Set<OrderItem>()
            .AsNoTracking()
            .Include(oi => oi.Product)
            .Where(oi => oi.Order.PaymentStatus == PaymentStatus.Completed &&
                  oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate &&
                  oi.Product.SellerId == request.SellerId)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new SellerTopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                QuantitySold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(p => p.Revenue)
            // ✅ BOLUM 12.0: Magic number config'den - SellerSettings kullanımı
            .Take(sellerConfig.TopProductsLimit)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var sellerProfile = await context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == request.SellerId, cancellationToken);

        // ✅ PERFORMANCE: Database'de distinct count yap (memory'de işlem YASAK)
        var uniqueCustomers = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId))
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new SellerPerformanceDto
        {
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            AverageOrderValue = averageOrderValue,
            ConversionRate = 0, // Bu hesaplama için daha fazla veri gerekir
            AverageRating = sellerProfile?.AverageRating ?? 0,
            TotalCustomers = uniqueCustomers,
            SalesByDate = salesByDate,
            TopProducts = topProducts
        };
    }
}
