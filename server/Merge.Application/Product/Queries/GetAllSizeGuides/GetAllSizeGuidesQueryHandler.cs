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

namespace Merge.Application.Product.Queries.GetAllSizeGuides;

public class GetAllSizeGuidesQueryHandler(
    IDbContext context,
    ILogger<GetAllSizeGuidesQueryHandler> logger,
    ICacheService cache,
    IOptions<CacheSettings> cacheSettings,
    IMapper mapper) : IRequestHandler<GetAllSizeGuidesQuery, IEnumerable<SizeGuideDto>>
{
    private readonly CacheSettings cacheConfig = cacheSettings.Value;

    private const string CACHE_KEY_ALL_SIZE_GUIDES = "size_guides_all";

    public async Task<IEnumerable<SizeGuideDto>> Handle(GetAllSizeGuidesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching all size guides");

        var cachedSizeGuides = await cache.GetAsync<IEnumerable<SizeGuideDto>>(CACHE_KEY_ALL_SIZE_GUIDES, cancellationToken);
        if (cachedSizeGuides is not null)
        {
            logger.LogInformation("Size guides retrieved from cache");
            return cachedSizeGuides;
        }

        logger.LogInformation("Cache miss for all size guides. Fetching from database.");

        var sizeGuides = await context.Set<SizeGuide>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(sg => sg.Category)
            .Include(sg => sg.Entries)
            .Where(sg => sg.IsActive)
            .ToListAsync(cancellationToken);

        var sizeGuideDtos = mapper.Map<IEnumerable<SizeGuideDto>>(sizeGuides).ToList();

        await cache.SetAsync(CACHE_KEY_ALL_SIZE_GUIDES, sizeGuideDtos, TimeSpan.FromMinutes(cacheConfig.SizeGuideCacheExpirationMinutes), cancellationToken);

        logger.LogInformation("Retrieved all size guides. Count: {Count}", sizeGuides.Count);

        return sizeGuideDtos;
    }
}
