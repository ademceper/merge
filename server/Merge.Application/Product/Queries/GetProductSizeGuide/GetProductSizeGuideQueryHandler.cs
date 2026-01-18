using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Product.Queries.GetProductSizeGuide;

public class GetProductSizeGuideQueryHandler(
    IDbContext context,
    ILogger<GetProductSizeGuideQueryHandler> logger,
    ICacheService cache,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetProductSizeGuideQuery, ProductSizeGuideDto?>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_PRODUCT_SIZE_GUIDE = "product_size_guide_";

    public async Task<ProductSizeGuideDto?> Handle(GetProductSizeGuideQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching product size guide. ProductId: {ProductId}", request.ProductId);

        var cacheKey = $"{CACHE_KEY_PRODUCT_SIZE_GUIDE}{request.ProductId}";
        var cachedProductSizeGuide = await cache.GetAsync<ProductSizeGuideDto>(cacheKey, cancellationToken);
        if (cachedProductSizeGuide is not null)
        {
            logger.LogInformation("Product size guide retrieved from cache. ProductId: {ProductId}", request.ProductId);
            return cachedProductSizeGuide;
        }

        logger.LogInformation("Cache miss for product size guide. ProductId: {ProductId}", request.ProductId);

        var productSizeGuide = await context.Set<ProductSizeGuide>()
            .AsNoTracking()
            .Include(psg => psg.Product)
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Category)
            .Include(psg => psg.SizeGuide)
                .ThenInclude(sg => sg.Entries)
            .FirstOrDefaultAsync(psg => psg.ProductId == request.ProductId, cancellationToken);

        if (productSizeGuide is null)
        {
            logger.LogWarning("Product size guide not found. ProductId: {ProductId}", request.ProductId);
            return null;
        }

        var sizeGuideDto = mapper.Map<SizeGuideDto>(productSizeGuide.SizeGuide);
        var productSizeGuideDto = new ProductSizeGuideDto(
            ProductId: productSizeGuide.ProductId,
            ProductName: productSizeGuide.Product.Name,
            SizeGuide: sizeGuideDto,
            CustomNotes: productSizeGuide.CustomNotes,
            FitDescription: productSizeGuide.FitDescription
        );

        await cache.SetAsync(cacheKey, productSizeGuideDto, TimeSpan.FromMinutes(cacheConfig.ProductSizeGuideCacheExpirationMinutes), cancellationToken);

        logger.LogInformation("Product size guide retrieved successfully. ProductId: {ProductId}", request.ProductId);

        return productSizeGuideDto;
    }
}
