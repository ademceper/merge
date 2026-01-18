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

namespace Merge.Application.Product.Queries.GetProductBundleById;

public class GetProductBundleByIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetProductBundleByIdQueryHandler> logger, ICacheService cache, IOptions<CacheSettings> cacheSettings) : IRequestHandler<GetProductBundleByIdQuery, ProductBundleDto?>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;


    private const string CACHE_KEY_BUNDLE_BY_ID = "bundle_";

    public async Task<ProductBundleDto?> Handle(GetProductBundleByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching product bundle by Id: {BundleId}", request.Id);

        var cacheKey = $"{CACHE_KEY_BUNDLE_BY_ID}{request.Id}";
        var cachedBundle = await cache.GetAsync<ProductBundleDto>(cacheKey, cancellationToken);
        if (cachedBundle is not null)
        {
            logger.LogInformation("Product bundle retrieved from cache. BundleId: {BundleId}", request.Id);
            return cachedBundle;
        }

        var bundle = await context.Set<ProductBundle>()
            .AsNoTracking()
            .Include(b => b.BundleItems)
                .ThenInclude(bi => bi.Product)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (bundle is null)
        {
            logger.LogWarning("Product bundle not found. BundleId: {BundleId}", request.Id);
            return null;
        }

        var bundleDto = mapper.Map<ProductBundleDto>(bundle);

        await cache.SetAsync(cacheKey, bundleDto, TimeSpan.FromMinutes(cacheConfig.ProductBundleCacheExpirationMinutes), cancellationToken);

        logger.LogInformation("Product bundle retrieved successfully. BundleId: {BundleId}", request.Id);

        return bundleDto;
    }
}
