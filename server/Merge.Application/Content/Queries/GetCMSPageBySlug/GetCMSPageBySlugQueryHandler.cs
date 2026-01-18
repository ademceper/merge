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

namespace Merge.Application.Content.Queries.GetCMSPageBySlug;

public class GetCMSPageBySlugQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetCMSPageBySlugQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetCMSPageBySlugQuery, CMSPageDto?>
{
    private const string CACHE_KEY_CMS_PAGE_BY_SLUG = "cms_page_slug_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public async Task<CMSPageDto?> Handle(GetCMSPageBySlugQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving CMS page with Slug: {Slug}", request.Slug);

        var cacheKey = $"{CACHE_KEY_CMS_PAGE_BY_SLUG}{request.Slug}";

        var cachedPage = await cache.GetAsync<CMSPageDto>(cacheKey, cancellationToken);
        if (cachedPage is not null)
        {
            logger.LogInformation("Cache hit for CMS page by slug. Slug: {Slug}", request.Slug);
            return cachedPage;
        }

        logger.LogInformation("Cache miss for CMS page by slug. Slug: {Slug}", request.Slug);

        var page = await context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .FirstOrDefaultAsync(p => p.Slug == request.Slug && p.Status == ContentStatus.Published, cancellationToken);

        if (page is null)
        {
            logger.LogWarning("CMS page not found with Slug: {Slug}", request.Slug);
            return null;
        }

        logger.LogInformation("Successfully retrieved CMS page {PageId} with Slug: {Slug}",
            page.Id, request.Slug);

        var pageDto = mapper.Map<CMSPageDto>(page);
        
        await cache.SetAsync(cacheKey, pageDto, CACHE_EXPIRATION, cancellationToken);

        return pageDto;
    }
}

