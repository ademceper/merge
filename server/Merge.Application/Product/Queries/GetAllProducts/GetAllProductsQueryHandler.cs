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

namespace Merge.Application.Product.Queries.GetAllProducts;

public class GetAllProductsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAllProductsQueryHandler> logger, ICacheService cache, IOptions<PaginationSettings> paginationSettings, IOptions<CacheSettings> cacheSettings) : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    private const string CACHE_KEY_ALL_PRODUCTS_PAGED = "products_all_paged";

    public async Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching all products. Page: {Page}, PageSize: {PageSize}",
            request.Page, request.PageSize);

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > paginationConfig.MaxPageSize
            ? paginationConfig.MaxPageSize
            : request.PageSize;

        var cacheKey = $"{CACHE_KEY_ALL_PRODUCTS_PAGED}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for all products (paged). Fetching from database.");

                var query = context.Set<ProductEntity>()
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Where(p => p.IsActive);

                var totalCount = await query.CountAsync(cancellationToken);

                var products = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var dtos = mapper.Map<IEnumerable<ProductDto>>(products);

                return new PagedResult<ProductDto>
                {
                    Items = dtos.ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            TimeSpan.FromMinutes(cacheConfig.ProductCacheExpirationMinutes),
            cancellationToken);

        return cachedResult!;
    }
}
