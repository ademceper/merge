using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Queries.GetSEOSettings;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetSEOSettingsQueryHandler : IRequestHandler<GetSEOSettingsQuery, SEOSettingsDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSEOSettingsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_SEO_SETTINGS = "seo_settings_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(30); // SEO settings change less frequently

    public GetSEOSettingsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSEOSettingsQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<SEOSettingsDto?> Handle(GetSEOSettingsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving SEO settings. PageType: {PageType}, EntityId: {EntityId}",
            request.PageType, request.EntityId);

        var cacheKey = $"{CACHE_KEY_SEO_SETTINGS}{request.PageType}_{request.EntityId?.ToString() ?? "null"}";

        // ✅ BOLUM 10.2: Redis distributed cache for SEO settings
        var cachedSettings = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for SEO settings. PageType: {PageType}, EntityId: {EntityId}",
                    request.PageType, request.EntityId);

                // ✅ PERFORMANCE: AsNoTracking for read-only queries
                var settings = await _context.Set<SEOSettings>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.PageType == request.PageType && 
                                            s.EntityId == request.EntityId, cancellationToken);

                if (settings == null)
                {
                    _logger.LogWarning("SEO settings not found. PageType: {PageType}, EntityId: {EntityId}",
                        request.PageType, request.EntityId);
                    return null;
                }

                return _mapper.Map<SEOSettingsDto>(settings);
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedSettings;
    }
}

