using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.SearchProducts;

public class SearchProductsQueryHandler(IDbContext context, IMapper mapper, ILogger<SearchProductsQueryHandler> logger, ICacheService cache, IOptions<PaginationSettings> paginationSettings, IOptions<CacheSettings> cacheSettings) : IRequestHandler<SearchProductsQuery, PagedResult<ProductDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_PRODUCTS_SEARCH = "products_search_";

    public async Task<PagedResult<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Searching products. Term: {SearchTerm}, Page: {Page}, PageSize: {PageSize}",
            request.SearchTerm, request.Page, request.PageSize);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > paginationConfig.MaxPageSize
            ? paginationConfig.MaxPageSize
            : request.PageSize;

        // Cache key includes search term (normalized to lowercase for consistency)
        var normalizedSearchTerm = request.SearchTerm.ToLowerInvariant();
        var cacheKey = $"{CACHE_KEY_PRODUCTS_SEARCH}{normalizedSearchTerm}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for product search. Term: {SearchTerm}, Page: {Page}, PageSize: {PageSize}",
                    request.SearchTerm, request.Page, request.PageSize);

                var query = context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.IsActive &&
                        (EF.Functions.ILike(p.Name, $"%{request.SearchTerm}%") ||
                         EF.Functions.ILike(p.Description, $"%{request.SearchTerm}%") ||
                         EF.Functions.ILike(p.Brand, $"%{request.SearchTerm}%")));

                var totalCount = await query.CountAsync(cancellationToken);

                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                logger.LogInformation(
                    "Product search completed. Term: {SearchTerm}, Results: {Count}, TotalCount: {TotalCount}",
                    request.SearchTerm, products.Count, totalCount);

                var dtos = mapper.Map<IEnumerable<ProductDto>>(products);

                return new PagedResult<ProductDto>
                {
                    Items = dtos.ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            TimeSpan.FromMinutes(cacheConfig.ProductSearchCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }
}
