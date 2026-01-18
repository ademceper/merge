using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetSalesAnalytics;

public class GetSalesAnalyticsQueryHandler(
    IDbContext context,
    ILogger<GetSalesAnalyticsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetSalesAnalyticsQuery, SalesAnalyticsDto>
{

    public async Task<SalesAnalyticsDto> Handle(GetSalesAnalyticsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching sales analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);
        
        var ordersQuery = context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate);

        var totalOrders = await ordersQuery.CountAsync(cancellationToken);
        var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        var totalTax = await ordersQuery.SumAsync(o => o.Tax, cancellationToken);
        var totalShipping = await ordersQuery.SumAsync(o => o.ShippingCost, cancellationToken);
        var totalDiscounts = await ordersQuery.SumAsync(o => (o.CouponDiscount ?? 0) + (o.GiftCardDiscount ?? 0), cancellationToken);

        logger.LogInformation("Sales analytics calculated. TotalRevenue: {TotalRevenue}, TotalOrders: {TotalOrders}",
            totalRevenue, totalOrders);

        // Revenue over time - Helper method çağrısı
        var revenueOverTime = await GetRevenueOverTimeAsync(request.StartDate, request.EndDate, cancellationToken);
        var topProducts = await GetTopProductsAsync(request.StartDate, request.EndDate, settings.Value.DefaultLimit, cancellationToken);
        var salesByCategory = await GetSalesByCategoryAsync(request.StartDate, request.EndDate, cancellationToken);
        
        return new SalesAnalyticsDto(
            request.StartDate,
            request.EndDate,
            totalRevenue,
            totalOrders,
            totalOrders > 0 ? totalRevenue / totalOrders : 0,
            totalTax,
            totalShipping,
            totalDiscounts,
            totalRevenue - totalDiscounts,
            revenueOverTime,
            new List<TimeSeriesDataPoint>(), // OrdersOverTime - şimdilik boş
            salesByCategory,
            topProducts
        );
    }

    private async Task<List<TimeSeriesDataPoint>> GetRevenueOverTimeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await context.Set<OrderEntity>()
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

    private async Task<List<TopProductDto>> GetTopProductsAsync(DateTime startDate, DateTime endDate, int limit, CancellationToken cancellationToken)
    {
        return await context.Set<OrderItem>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.SKU })
            .Select(g => new TopProductDto(
                g.Key.ProductId,
                g.Key.Name,
                g.Key.SKU,
                g.Sum(oi => oi.Quantity),
                g.Sum(oi => oi.TotalPrice),
                g.Average(oi => oi.UnitPrice)
            ))
            .OrderByDescending(p => p.Revenue)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<CategorySalesDto>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await context.Set<OrderItem>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate && oi.Product.Category != null)
            .GroupBy(oi => new { oi.Product.CategoryId, CategoryName = oi.Product.Category!.Name })
            .Select(g => new CategorySalesDto(
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Sum(oi => oi.TotalPrice),
                g.Select(oi => oi.OrderId).Distinct().Count(),
                g.Sum(oi => oi.Quantity)
            ))
            .OrderByDescending(c => c.Revenue)
            .ToListAsync(cancellationToken);
    }
}

