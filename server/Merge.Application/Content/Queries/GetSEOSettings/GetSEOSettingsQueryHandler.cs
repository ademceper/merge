using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetSEOSettings;

public class GetSEOSettingsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetSEOSettingsQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetSEOSettingsQuery, SEOSettingsDto?>
{
    private const string CACHE_KEY_SEO_SETTINGS = "seo_settings_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(30); // SEO settings change less frequently

    public async Task<SEOSettingsDto?> Handle(GetSEOSettingsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving SEO settings. PageType: {PageType}, EntityId: {EntityId}",
            request.PageType, request.EntityId);

        var cacheKey = $"{CACHE_KEY_SEO_SETTINGS}{request.PageType}_{request.EntityId?.ToString() ?? "null"}";

        var cachedSettings = await cache.GetOrCreateNullableAsync<SEOSettingsDto>(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for SEO settings. PageType: {PageType}, EntityId: {EntityId}",
                    request.PageType, request.EntityId);

                var settings = await context.Set<SEOSettings>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.PageType == request.PageType && 
                                            s.EntityId == request.EntityId, cancellationToken);

                if (settings is null)
                {
                    logger.LogWarning("SEO settings not found. PageType: {PageType}, EntityId: {EntityId}",
                        request.PageType, request.EntityId);
                    return null;
                }

                return mapper.Map<SEOSettingsDto>(settings);
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedSettings;
    }
}

