using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Queries.GetLandingPageAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetLandingPageAnalyticsQueryHandler : IRequestHandler<GetLandingPageAnalyticsQuery, LandingPageAnalyticsDto>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLandingPageAnalyticsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ANALYTICS = "landing_page_analytics_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public GetLandingPageAnalyticsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetLandingPageAnalyticsQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<LandingPageAnalyticsDto> Handle(GetLandingPageAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving landing page analytics. LandingPageId: {LandingPageId}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.Id, request.StartDate, request.EndDate);

        var cacheKey = $"{CACHE_KEY_ANALYTICS}{request.Id}_{request.StartDate?.ToString("yyyy-MM-dd") ?? "all"}_{request.EndDate?.ToString("yyyy-MM-dd") ?? "all"}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedAnalytics = await _cache.GetAsync<LandingPageAnalyticsDto>(cacheKey, cancellationToken);
        if (cachedAnalytics != null)
        {
            _logger.LogInformation("Cache hit for landing page analytics. LandingPageId: {LandingPageId}", request.Id);
            return cachedAnalytics;
        }

        _logger.LogInformation("Cache miss for landing page analytics. LandingPageId: {LandingPageId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var landingPage = await _context.Set<LandingPage>()
            .AsNoTracking()
            .Include(lp => lp.Variants.Where(v => v.IsActive))
            .FirstOrDefaultAsync(lp => lp.Id == request.Id, cancellationToken);

        if (landingPage == null)
        {
            _logger.LogWarning("Landing page not found for analytics. LandingPageId: {LandingPageId}", request.Id);
            throw new NotFoundException("Landing Page", request.Id);
        }

        var start = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = request.EndDate ?? DateTime.UtcNow;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var variants = landingPage.Variants != null && landingPage.Variants.Any()
            ? landingPage.Variants.Select(v => _mapper.Map<LandingPageVariantDto>(v)).ToList()
            : new List<LandingPageVariantDto>();

        var analytics = new LandingPageAnalyticsDto(
            landingPage.Id,
            landingPage.Name,
            landingPage.ViewCount,
            landingPage.ConversionCount,
            landingPage.ConversionRate,
            new Dictionary<string, int>(), // ViewsByDate - Gerçek implementasyonda hesaplanmalı (analytics service'den gelecek)
            new Dictionary<string, int>(), // ConversionsByDate - Gerçek implementasyonda hesaplanmalı (analytics service'den gelecek)
            variants
        );

        _logger.LogInformation("Successfully retrieved landing page analytics. LandingPageId: {LandingPageId}", request.Id);

        // ✅ BOLUM 10.2: Cache the result
        await _cache.SetAsync(cacheKey, analytics, CACHE_EXPIRATION, cancellationToken);

        return analytics;
    }
}

