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
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.LandingPage>;

namespace Merge.Application.Content.Queries.GetLandingPageById;

public class GetLandingPageByIdQueryHandler(
    IDbContext context,
    IRepository landingPageRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<GetLandingPageByIdQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetLandingPageByIdQuery, LandingPageDto?>
{
    private const string CACHE_KEY_PAGE_BY_ID = "landing_page_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<LandingPageDto?> Handle(GetLandingPageByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving landing page with Id: {LandingPageId}, TrackView: {TrackView}", request.Id, request.TrackView);

        var cacheKey = $"{CACHE_KEY_PAGE_BY_ID}{request.Id}";

        var cachedPage = await cache.GetAsync<LandingPageDto>(cacheKey, cancellationToken);
        if (cachedPage is not null && !request.TrackView)
        {
            logger.LogInformation("Cache hit for landing page. LandingPageId: {LandingPageId}", request.Id);
            return cachedPage;
        }

        logger.LogInformation("Cache miss for landing page. LandingPageId: {LandingPageId}", request.Id);

        var landingPage = request.TrackView
            ? await context.Set<LandingPage>()
                .Include(lp => lp.Author)
                .Include(lp => lp.VariantOf)
                .FirstOrDefaultAsync(lp => lp.Id == request.Id, cancellationToken)
            : await context.Set<LandingPage>()
                .AsNoTracking()
                .Include(lp => lp.Author)
                .Include(lp => lp.VariantOf)
                .FirstOrDefaultAsync(lp => lp.Id == request.Id, cancellationToken);

        if (landingPage is null)
        {
            logger.LogWarning("Landing page not found with Id: {LandingPageId}", request.Id);
            return null;
        }

        if (request.TrackView && landingPage.Status == ContentStatus.Published && landingPage.IsActive)
        {
            landingPage.IncrementViewCount();
            await landingPageRepository.UpdateAsync(landingPage, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Successfully retrieved landing page {LandingPageId}", request.Id);

        var pageDto = mapper.Map<LandingPageDto>(landingPage);

        if (!request.TrackView)
        {
            await cache.SetAsync(cacheKey, pageDto, CACHE_EXPIRATION, cancellationToken);
        }

        return pageDto;
    }
}

