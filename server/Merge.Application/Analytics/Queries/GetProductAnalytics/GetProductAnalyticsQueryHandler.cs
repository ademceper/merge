using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetProductAnalytics;

public class GetProductAnalyticsQueryHandler(
    IDbContext context,
    ILogger<GetProductAnalyticsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetProductAnalyticsQuery, ProductAnalyticsDto>
{

    public async Task<ProductAnalyticsDto> Handle(GetProductAnalyticsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching product analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        var totalProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.IsActive, cancellationToken);

        var outOfStock = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StockQuantity == 0, cancellationToken);

        var lowStock = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StockQuantity > 0 && p.StockQuantity < settings.Value.LowStockThreshold, cancellationToken);

        var totalValue = await context.Set<ProductEntity>()
            .AsNoTracking()
            .SumAsync(p => p.Price * p.StockQuantity, cancellationToken);

        var end = request.EndDate ?? DateTime.UtcNow;
        var start = request.StartDate ?? end.AddDays(-settings.Value.DefaultPeriodDays);

        var bestSellers = await GetBestSellersAsync(settings.Value.MaxQueryLimit, cancellationToken);
        var worstPerformers = await GetWorstPerformersAsync(settings.Value.MaxQueryLimit, cancellationToken);
        var categoryPerformance = await GetCategoryPerformanceAsync(cancellationToken);
        
        return new ProductAnalyticsDto(
            start,
            end,
            totalProducts,
            activeProducts,
            outOfStock,
            lowStock,
            totalValue,
            bestSellers,
            worstPerformers,
            categoryPerformance
        );
    }

    private async Task<List<TopProductDto>> GetBestSellersAsync(int limit, CancellationToken cancellationToken)
    {
        var last30Days = DateTime.UtcNow.AddDays(-settings.Value.DefaultPeriodDays);
        return await context.Set<OrderItem>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedAt >= last30Days)
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

    private async Task<List<TopProductDto>> GetWorstPerformersAsync(int limit, CancellationToken cancellationToken)
    {
        var last30Days = DateTime.UtcNow.AddDays(-settings.Value.DefaultPeriodDays);
        return await context.Set<OrderItem>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(oi => oi.Product)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedAt >= last30Days)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.SKU })
            .Select(g => new TopProductDto(
                g.Key.ProductId,
                g.Key.Name,
                g.Key.SKU,
                g.Sum(oi => oi.Quantity),
                g.Sum(oi => oi.TotalPrice),
                g.Average(oi => oi.UnitPrice)
            ))
            .OrderBy(p => p.Revenue)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<ProductCategoryPerformanceDto>> GetCategoryPerformanceAsync(CancellationToken cancellationToken)
    {
        return await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.Category != null)
            .GroupBy(p => new { p.CategoryId, CategoryName = p.Category!.Name })
            .Select(g => new ProductCategoryPerformanceDto(
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Count(),
                g.Sum(p => p.StockQuantity),
                g.Average(p => p.Price),
                g.Sum(p => p.Price * p.StockQuantity)
            ))
            .OrderByDescending(c => c.TotalValue)
            .ToListAsync(cancellationToken);
    }
}

