using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetAllProductBundles;

public class GetAllProductBundlesQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAllProductBundlesQueryHandler> logger, ICacheService cache, IOptions<CacheSettings> cacheSettings) : IRequestHandler<GetAllProductBundlesQuery, IEnumerable<ProductBundleDto>>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;


    private const string CACHE_KEY_ALL_BUNDLES = "bundles_all";
    private const string CACHE_KEY_ACTIVE_BUNDLES = "bundles_active";

    public async Task<IEnumerable<ProductBundleDto>> Handle(GetAllProductBundlesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching all product bundles. ActiveOnly: {ActiveOnly}", request.ActiveOnly);

        var cacheKey = request.ActiveOnly ? CACHE_KEY_ACTIVE_BUNDLES : CACHE_KEY_ALL_BUNDLES;
        var cachedBundles = await cache.GetAsync<IEnumerable<ProductBundleDto>>(cacheKey, cancellationToken);
        if (cachedBundles != null)
        {
            logger.LogInformation("Product bundles retrieved from cache. ActiveOnly: {ActiveOnly}", request.ActiveOnly);
            return cachedBundles;
        }

        IQueryable<ProductBundle> query = context.Set<ProductBundle>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product);

        if (request.ActiveOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(b => b.IsActive &&
                  (!b.StartDate.HasValue || b.StartDate.Value <= now) &&
                  (!b.EndDate.HasValue || b.EndDate.Value >= now));
        }

        var bundles = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        var bundleDtos = mapper.Map<IEnumerable<ProductBundleDto>>(bundles).ToList();

        await cache.SetAsync(cacheKey, bundleDtos, TimeSpan.FromMinutes(cacheConfig.ProductBundleCacheExpirationMinutes), cancellationToken);

        logger.LogInformation("Retrieved all product bundles. Count: {Count}, ActiveOnly: {ActiveOnly}",
            bundles.Count, request.ActiveOnly);

        return bundleDtos;
    }
}
