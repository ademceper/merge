using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Queries.GetBannerById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetBannerByIdQueryHandler : IRequestHandler<GetBannerByIdQuery, BannerDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBannerByIdQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_BANNER_BY_ID = "banner_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15); // Banners change less frequently

    public GetBannerByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetBannerByIdQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<BannerDto?> Handle(GetBannerByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving banner with Id: {BannerId}", request.Id);

        var cacheKey = $"{CACHE_KEY_BANNER_BY_ID}{request.Id}";

        // ✅ BOLUM 10.2: Redis distributed cache for single banner
        var cachedBanner = await _cache.GetAsync<BannerDto>(cacheKey, cancellationToken);
        if (cachedBanner != null)
        {
            _logger.LogInformation("Cache hit for banner. BannerId: {BannerId}", request.Id);
            return cachedBanner;
        }

        _logger.LogInformation("Cache miss for banner. BannerId: {BannerId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var banner = await _context.Set<Banner>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (banner == null)
        {
            _logger.LogWarning("Banner not found with Id: {BannerId}", request.Id);
            return null;
        }

        _logger.LogInformation("Successfully retrieved banner {BannerId} with Title: {Title}",
            request.Id, banner.Title);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var bannerDto = _mapper.Map<BannerDto>(banner);
        
        // Cache the result
        await _cache.SetAsync(cacheKey, bannerDto, CACHE_EXPIRATION, cancellationToken);

        return bannerDto;
    }
}

