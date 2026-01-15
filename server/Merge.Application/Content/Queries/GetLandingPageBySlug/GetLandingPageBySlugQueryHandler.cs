using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetLandingPageBySlug;

public class GetLandingPageBySlugQueryHandler(
    IDbContext context,
    Merge.Application.Interfaces.IRepository<LandingPage> landingPageRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<GetLandingPageBySlugQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetLandingPageBySlugQuery, LandingPageDto?>
{
    private const string CACHE_KEY_PAGE_BY_SLUG = "landing_page_slug_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<LandingPageDto?> Handle(GetLandingPageBySlugQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving landing page with Slug: {Slug}, TrackView: {TrackView}", request.Slug, request.TrackView);

        var cacheKey = $"{CACHE_KEY_PAGE_BY_SLUG}{request.Slug}";

        var cachedPage = await cache.GetAsync<LandingPageDto>(cacheKey, cancellationToken);
        if (cachedPage != null && !request.TrackView)
        {
            logger.LogInformation("Cache hit for landing page. Slug: {Slug}", request.Slug);
            return cachedPage;
        }

        logger.LogInformation("Cache miss for landing page. Slug: {Slug}", request.Slug);

        var landingPage = request.TrackView
            ? await context.Set<LandingPage>()
                .Include(lp => lp.Author)
                .Include(lp => lp.VariantOf)
                .FirstOrDefaultAsync(lp => lp.Slug == request.Slug && lp.Status == ContentStatus.Published && lp.IsActive, cancellationToken)
            : await context.Set<LandingPage>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(lp => lp.Author)
                .Include(lp => lp.VariantOf)
                .FirstOrDefaultAsync(lp => lp.Slug == request.Slug && lp.Status == ContentStatus.Published && lp.IsActive, cancellationToken);

        if (landingPage == null)
        {
            logger.LogWarning("Landing page not found with Slug: {Slug}", request.Slug);
            return null;
        }

        if (landingPage.EnableABTesting && landingPage.Variants != null && landingPage.Variants.Any())
        {
            var variants = landingPage.Variants.Where(v => v.IsActive).ToList();
            if (variants.Any())
            {
                var random = Random.Shared;
                var selectedVariant = variants.OrderBy(v => random.Next()).FirstOrDefault();
                if (selectedVariant != null)
                {
                    landingPage = selectedVariant;
                }
            }
        }

        if (request.TrackView)
        {
            landingPage.IncrementViewCount();
            await landingPageRepository.UpdateAsync(landingPage, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Successfully retrieved landing page {Slug}", request.Slug);

        var pageDto = mapper.Map<LandingPageDto>(landingPage);

        if (!request.TrackView)
        {
            await cache.SetAsync(cacheKey, pageDto, CACHE_EXPIRATION, cancellationToken);
        }

        return pageDto;
    }
}

