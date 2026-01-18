using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.ML.Helpers;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Queries.OptimizePricesForCategory;

public class OptimizePricesForCategoryQueryHandler(IDbContext context, ILogger<OptimizePricesForCategoryQueryHandler> logger, IOptions<PaginationSettings> paginationSettings, PriceOptimizationHelper helper) : IRequestHandler<OptimizePricesForCategoryQuery, PagedResult<PriceOptimizationDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<PriceOptimizationDto>> Handle(OptimizePricesForCategoryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Optimizing prices for category. CategoryId: {CategoryId}, Page: {Page}, PageSize: {PageSize}",
            request.CategoryId, request.Page, request.PageSize);

        var page = request.Page;
        var pageSize = request.PageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        var products = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == request.CategoryId && p.IsActive)
            .ToListAsync(cancellationToken);

        var allSimilarProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => p.CategoryId == request.CategoryId && p.IsActive)
            .ToListAsync(cancellationToken);

        // Not: Bu durumda entity grouping yapılıyor, database'de ToDictionaryAsync yapılamaz
        // Ancak bu minimal bir işlem ve business logic için gerekli (ML algoritması için grouping)
        // Ancak bu durumda zaten tek category olduğu için grouping gereksiz, direkt liste kullanılabilir
        // Ancak kod tutarlılığı için aynı pattern kullanılıyor
        var similarProductsByCategory = allSimilarProducts
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<PriceOptimizationDto> results = [];

        foreach (var product in products)
        {
            var similarProducts = similarProductsByCategory.TryGetValue(product.CategoryId, out var similar) 
                ? similar.Where(p => p.Id != product.Id).ToList() 
                : [];
            
            var recommendation = await helper.CalculateOptimalPriceAsync(product, similarProducts, cancellationToken);
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

        // Not: Bu durumda `results` zaten memory'de (List), bu yüzden bu minimal bir işlem
        // Ancak business logic için gerekli (sıralama için)
        var orderedResults = results.OrderByDescending(r => r.ExpectedRevenueChange).ToList();

        var totalCount = orderedResults.Count;
        var pagedResults = orderedResults
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        logger.LogInformation("Prices optimized for category. CategoryId: {CategoryId}, TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}",
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
