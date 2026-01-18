using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetProductById;

public class GetProductByIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetProductByIdQueryHandler> logger, ICacheService cache, IOptions<CacheSettings> cacheSettings) : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;


    private const string CACHE_KEY_PRODUCT_BY_ID = "product_";

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching product by Id: {ProductId}", request.ProductId);

        var cacheKey = $"{CACHE_KEY_PRODUCT_BY_ID}{request.ProductId}";

        var cachedProduct = await cache.GetAsync<ProductDto>(cacheKey, cancellationToken);
        if (cachedProduct != null)
        {
            logger.LogInformation("Cache hit for product. ProductId: {ProductId}", request.ProductId);
            return cachedProduct;
        }

        logger.LogInformation("Cache miss for product. ProductId: {ProductId}", request.ProductId);

        var product = await context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product == null)
        {
            logger.LogWarning("Product not found with Id: {ProductId}", request.ProductId);
            return null;
        }

        var productDto = mapper.Map<ProductDto>(product);
        
        // Cache the result
        var cacheExpiration = TimeSpan.FromMinutes(cacheConfig.ProductCacheExpirationMinutes);
        await cache.SetAsync(cacheKey, productDto, cacheExpiration, cancellationToken);

        return productDto;
    }
}
