using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetBannerById;

public class GetBannerByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetBannerByIdQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetBannerByIdQuery, BannerDto?>
{
    private const string CACHE_KEY_BANNER_BY_ID = "banner_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15); // Banners change less frequently

    public async Task<BannerDto?> Handle(GetBannerByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving banner with Id: {BannerId}", request.Id);

        var cacheKey = $"{CACHE_KEY_BANNER_BY_ID}{request.Id}";

        var cachedBanner = await cache.GetAsync<BannerDto>(cacheKey, cancellationToken);
        if (cachedBanner != null)
        {
            logger.LogInformation("Cache hit for banner. BannerId: {BannerId}", request.Id);
            return cachedBanner;
        }

        logger.LogInformation("Cache miss for banner. BannerId: {BannerId}", request.Id);

        var banner = await context.Set<Banner>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (banner == null)
        {
            logger.LogWarning("Banner not found with Id: {BannerId}", request.Id);
            return null;
        }

        logger.LogInformation("Successfully retrieved banner {BannerId} with Title: {Title}",
            request.Id, banner.Title);

        var bannerDto = mapper.Map<BannerDto>(banner);
        
        // Cache the result
        await cache.SetAsync(cacheKey, bannerDto, CACHE_EXPIRATION, cancellationToken);

        return bannerDto;
    }
}

