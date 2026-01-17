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

namespace Merge.Application.Product.Queries.GetSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetSizeGuideQueryHandler(
    IDbContext context,
    ILogger<GetSizeGuideQueryHandler> logger,
    ICacheService cache,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetSizeGuideQuery, SizeGuideDto?>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_SIZE_GUIDE_BY_ID = "size_guide_";

    public async Task<SizeGuideDto?> Handle(GetSizeGuideQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching size guide by Id: {SizeGuideId}", request.Id);

        // ✅ BOLUM 10.1: Cache-Aside Pattern
        var cacheKey = $"{CACHE_KEY_SIZE_GUIDE_BY_ID}{request.Id}";
        var cachedSizeGuide = await cache.GetAsync<SizeGuideDto>(cacheKey, cancellationToken);
        if (cachedSizeGuide != null)
        {
            logger.LogInformation("Size guide retrieved from cache. SizeGuideId: {SizeGuideId}", request.Id);
            return cachedSizeGuide;
        }

        logger.LogInformation("Cache miss for size guide. SizeGuideId: {SizeGuideId}", request.Id);

        var sizeGuide = await context.Set<SizeGuide>()
            .AsNoTracking()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .FirstOrDefaultAsync(sg => sg.Id == request.Id, cancellationToken);

        if (sizeGuide == null)
        {
            logger.LogWarning("Size guide not found with Id: {SizeGuideId}", request.Id);
            return null;
        }

        var sizeGuideDto = mapper.Map<SizeGuideDto>(sizeGuide);

        // ✅ BOLUM 10.1: Cache-Aside Pattern - Cache'e yaz
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
        await cache.SetAsync(cacheKey, sizeGuideDto, TimeSpan.FromMinutes(cacheConfig.SizeGuideCacheExpirationMinutes), cancellationToken);

        logger.LogInformation("Size guide retrieved successfully. SizeGuideId: {SizeGuideId}", request.Id);

        return sizeGuideDto;
    }
}
