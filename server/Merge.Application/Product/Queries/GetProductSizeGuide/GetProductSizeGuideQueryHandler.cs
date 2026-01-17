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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
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

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        var cacheKey = $"{CACHE_KEY_PRODUCT_SIZE_GUIDE}{request.ProductId}";
        var cachedProductSizeGuide = await cache.GetAsync<ProductSizeGuideDto>(cacheKey, cancellationToken);
        if (cachedProductSizeGuide != null)
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

        if (productSizeGuide == null)
        {
            logger.LogWarning("Product size guide not found. ProductId: {ProductId}", request.ProductId);
            return null;
        }

        var sizeGuideDto = mapper.Map<SizeGuideDto>(productSizeGuide.SizeGuide);
        // ✅ BOLUM 7.1.5: Records - Record constructor kullanımı (object initializer YASAK)
        var productSizeGuideDto = new ProductSizeGuideDto(
            ProductId: productSizeGuide.ProductId,
            ProductName: productSizeGuide.Product.Name,
            SizeGuide: sizeGuideDto,
            CustomNotes: productSizeGuide.CustomNotes,
            FitDescription: productSizeGuide.FitDescription
        );

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
        await cache.SetAsync(cacheKey, productSizeGuideDto, TimeSpan.FromMinutes(cacheConfig.ProductSizeGuideCacheExpirationMinutes), cancellationToken);

        logger.LogInformation("Product size guide retrieved successfully. ProductId: {ProductId}", request.ProductId);

        return productSizeGuideDto;
    }
}
