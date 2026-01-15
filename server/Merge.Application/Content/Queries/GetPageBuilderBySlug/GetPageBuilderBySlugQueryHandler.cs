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

namespace Merge.Application.Content.Queries.GetPageBuilderBySlug;

public class GetPageBuilderBySlugQueryHandler(
    IDbContext context,
    Merge.Application.Interfaces.IRepository<PageBuilder> pageBuilderRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<GetPageBuilderBySlugQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetPageBuilderBySlugQuery, PageBuilderDto?>
{
    private const string CACHE_KEY_PAGE_BY_SLUG = "page_builder_slug_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<PageBuilderDto?> Handle(GetPageBuilderBySlugQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving page builder with Slug: {Slug}, TrackView: {TrackView}", request.Slug, request.TrackView);

        var cacheKey = $"{CACHE_KEY_PAGE_BY_SLUG}{request.Slug}";

        var cachedPage = await cache.GetAsync<PageBuilderDto>(cacheKey, cancellationToken);
        if (cachedPage != null && !request.TrackView)
        {
            logger.LogInformation("Cache hit for page builder. Slug: {Slug}", request.Slug);
            return cachedPage;
        }

        logger.LogInformation("Cache miss for page builder. Slug: {Slug}", request.Slug);

        var pageBuilder = request.TrackView
            ? await context.Set<PageBuilder>()
                .Include(pb => pb.Author)
                .FirstOrDefaultAsync(pb => pb.Slug == request.Slug && pb.Status == ContentStatus.Published, cancellationToken)
            : await context.Set<PageBuilder>()
                .AsNoTracking()
                .Include(pb => pb.Author)
                .FirstOrDefaultAsync(pb => pb.Slug == request.Slug && pb.Status == ContentStatus.Published, cancellationToken);

        if (pageBuilder == null)
        {
            logger.LogWarning("Page builder not found with Slug: {Slug}", request.Slug);
            return null;
        }

        if (request.TrackView)
        {
            pageBuilder.IncrementViewCount();
            await pageBuilderRepository.UpdateAsync(pageBuilder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Successfully retrieved page builder {Slug}", request.Slug);

        var pageDto = mapper.Map<PageBuilderDto>(pageBuilder);

        if (!request.TrackView)
        {
            await cache.SetAsync(cacheKey, pageDto, CACHE_EXPIRATION, cancellationToken);
        }

        return pageDto;
    }
}

