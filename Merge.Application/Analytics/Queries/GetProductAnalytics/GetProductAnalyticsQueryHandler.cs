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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetProductAnalyticsQueryHandler : IRequestHandler<GetProductAnalyticsQuery, ProductAnalyticsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetProductAnalyticsQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;

    public GetProductAnalyticsQueryHandler(
        IDbContext context,
        ILogger<GetProductAnalyticsQueryHandler> logger,
        IOptions<AnalyticsSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<ProductAnalyticsDto> Handle(GetProductAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching product analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: Database'de aggregate query kullan (tüm ürünleri çekmek yerine)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted check (Global Query Filter handles it)
        var totalProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var activeProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.IsActive, cancellationToken);

        var outOfStock = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StockQuantity == 0, cancellationToken);

        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var lowStock = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.StockQuantity > 0 && p.StockQuantity < _settings.LowStockThreshold, cancellationToken);

        var totalValue = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .SumAsync(p => p.Price * p.StockQuantity, cancellationToken);

        var end = request.EndDate ?? DateTime.UtcNow;
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var start = request.StartDate ?? end.AddDays(-_settings.DefaultPeriodDays);

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var bestSellers = await GetBestSellersAsync(_settings.MaxQueryLimit, cancellationToken);
        var worstPerformers = await GetWorstPerformersAsync(_settings.MaxQueryLimit, cancellationToken);
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
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var last30Days = DateTime.UtcNow.AddDays(-_settings.DefaultPeriodDays);
        return await _context.Set<OrderItem>()
            .AsNoTracking()
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
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var last30Days = DateTime.UtcNow.AddDays(-_settings.DefaultPeriodDays);
        return await _context.Set<OrderItem>()
            .AsNoTracking()
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
        return await _context.Set<ProductEntity>()
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

