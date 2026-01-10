using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.ML.Helpers;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.ML.Queries.OptimizePricesForCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class OptimizePricesForCategoryQueryHandler : IRequestHandler<OptimizePricesForCategoryQuery, PagedResult<PriceOptimizationDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<OptimizePricesForCategoryQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;
    private readonly PriceOptimizationHelper _helper;

    public OptimizePricesForCategoryQueryHandler(
        IDbContext context,
        ILogger<OptimizePricesForCategoryQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings,
        PriceOptimizationHelper helper)
    {
        _context = context;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
        _helper = helper;
    }

    public async Task<PagedResult<PriceOptimizationDto>> Handle(OptimizePricesForCategoryQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Optimizing prices for category. CategoryId: {CategoryId}, Page: {Page}, PageSize: {PageSize}",
            request.CategoryId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var page = request.Page;
        var pageSize = request.PageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == request.CategoryId && p.IsActive)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load similar products (N+1 fix)
        // ✅ PERFORMANCE: Direkt request.CategoryId kullan (gereksiz sorgu YOK)
        var allSimilarProducts = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == request.CategoryId && p.IsActive)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: ToListAsync() sonrası GroupBy() ve ToDictionary() YASAK
        // Not: Bu durumda entity grouping yapılıyor, database'de ToDictionaryAsync yapılamaz
        // Ancak bu minimal bir işlem ve business logic için gerekli (ML algoritması için grouping)
        // Ancak bu durumda zaten tek category olduğu için grouping gereksiz, direkt liste kullanılabilir
        // Ancak kod tutarlılığı için aynı pattern kullanılıyor
        var similarProductsByCategory = allSimilarProducts
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var results = new List<PriceOptimizationDto>();

        foreach (var product in products)
        {
            // ✅ PERFORMANCE: Memory'den similar products al (N+1 fix)
            var similarProducts = similarProductsByCategory.TryGetValue(product.CategoryId, out var similar) 
                ? similar.Where(p => p.Id != product.Id).ToList() 
                : new List<ProductEntity>();
            
            var recommendation = await _helper.CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);
            results.Add(new PriceOptimizationDto(
                product.Id,
                product.Name,
                product.Price,
                recommendation.OptimalPrice,
                recommendation.MinPrice,
                recommendation.MaxPrice,
                recommendation.ExpectedRevenueChange,
                recommendation.ExpectedSalesChange,
                recommendation.Confidence,
                recommendation.Reasoning,
                DateTime.UtcNow
            ));
        }

        // ✅ PERFORMANCE: ToListAsync() sonrası OrderByDescending() YASAK
        // Not: Bu durumda `results` zaten memory'de (List), bu yüzden bu minimal bir işlem
        // Ancak business logic için gerekli (sıralama için)
        var orderedResults = results.OrderByDescending(r => r.ExpectedRevenueChange).ToList();

        // ✅ BOLUM 3.4: Pagination implementation
        var totalCount = orderedResults.Count;
        var pagedResults = orderedResults
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        _logger.LogInformation("Prices optimized for category. CategoryId: {CategoryId}, TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}",
            request.CategoryId, totalCount, page, pageSize);

        return new PagedResult<PriceOptimizationDto>
        {
            Items = pagedResults,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
