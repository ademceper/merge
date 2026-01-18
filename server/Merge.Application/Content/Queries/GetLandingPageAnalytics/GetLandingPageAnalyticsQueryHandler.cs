using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetLandingPageAnalytics;

public class GetLandingPageAnalyticsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetLandingPageAnalyticsQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetLandingPageAnalyticsQuery, LandingPageAnalyticsDto>
{
    private const string CACHE_KEY_ANALYTICS = "landing_page_analytics_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<LandingPageAnalyticsDto> Handle(GetLandingPageAnalyticsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving landing page analytics. LandingPageId: {LandingPageId}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.Id, request.StartDate, request.EndDate);

        var cacheKey = $"{CACHE_KEY_ANALYTICS}{request.Id}_{request.StartDate?.ToString("yyyy-MM-dd") ?? "all"}_{request.EndDate?.ToString("yyyy-MM-dd") ?? "all"}";

        var cachedAnalytics = await cache.GetAsync<LandingPageAnalyticsDto>(cacheKey, cancellationToken);
        if (cachedAnalytics is not null)
        {
            logger.LogInformation("Cache hit for landing page analytics. LandingPageId: {LandingPageId}", request.Id);
            return cachedAnalytics;
        }

        logger.LogInformation("Cache miss for landing page analytics. LandingPageId: {LandingPageId}", request.Id);

        var landingPage = await context.Set<LandingPage>()
            .AsNoTracking()
            .Include(lp => lp.Variants.Where(v => v.IsActive))
            .FirstOrDefaultAsync(lp => lp.Id == request.Id, cancellationToken);

        if (landingPage is null)
        {
            logger.LogWarning("Landing page not found for analytics. LandingPageId: {LandingPageId}", request.Id);
            throw new NotFoundException("Landing Page", request.Id);
        }

        var start = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = request.EndDate ?? DateTime.UtcNow;

        var variants = landingPage.Variants is not null && landingPage.Variants.Any()
            ? landingPage.Variants.Select(v => mapper.Map<LandingPageVariantDto>(v)).ToList()
            : new List<LandingPageVariantDto>();

        var analytics = new LandingPageAnalyticsDto(
            landingPage.Id,
            landingPage.Name,
            landingPage.ViewCount,
            landingPage.ConversionCount,
            landingPage.ConversionRate,
            new Dictionary<string, int>(),
            new Dictionary<string, int>(),
            variants
        );

        logger.LogInformation("Successfully retrieved landing page analytics. LandingPageId: {LandingPageId}", request.Id);

        await cache.SetAsync(cacheKey, analytics, CACHE_EXPIRATION, cancellationToken);

        return analytics;
    }
}

