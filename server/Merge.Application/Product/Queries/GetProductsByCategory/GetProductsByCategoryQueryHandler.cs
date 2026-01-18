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

namespace Merge.Application.Product.Queries.GetProductsByCategory;

public class GetProductsByCategoryQueryHandler(IDbContext context, IMapper mapper, ILogger<GetProductsByCategoryQueryHandler> logger, ICacheService cache, IOptions<PaginationSettings> paginationSettings, IOptions<CacheSettings> cacheSettings) : IRequestHandler<GetProductsByCategoryQuery, PagedResult<ProductDto>>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    private const string CACHE_KEY_PRODUCTS_BY_CATEGORY = "products_by_category_";

    public async Task<PagedResult<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching products by category. CategoryId: {CategoryId}, Page: {Page}, PageSize: {PageSize}",
            request.CategoryId, request.Page, request.PageSize);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > paginationConfig.MaxPageSize
            ? paginationConfig.MaxPageSize
            : request.PageSize;

        var cacheKey = $"{CACHE_KEY_PRODUCTS_BY_CATEGORY}{request.CategoryId}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for products by category. CategoryId: {CategoryId}, Page: {Page}, PageSize: {PageSize}",
                    request.CategoryId, request.Page, request.PageSize);

                var query = context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && p.CategoryId == request.CategoryId);

                var totalCount = await query.CountAsync(cancellationToken);

                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                logger.LogInformation(
                    "Retrieved products by category. CategoryId: {CategoryId}, Count: {Count}, TotalCount: {TotalCount}",
                    request.CategoryId, products.Count, totalCount);

                var dtos = mapper.Map<IEnumerable<ProductDto>>(products);

                return new PagedResult<ProductDto>
                {
                    Items = dtos.ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            TimeSpan.FromMinutes(cacheConfig.ProductCategoryCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }
}
