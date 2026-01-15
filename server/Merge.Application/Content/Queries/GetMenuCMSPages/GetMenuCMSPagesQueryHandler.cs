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

namespace Merge.Application.Content.Queries.GetMenuCMSPages;

public class GetMenuCMSPagesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetMenuCMSPagesQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetMenuCMSPagesQuery, IEnumerable<CMSPageDto>>
{
    private const string CACHE_KEY_MENU_PAGES = "cms_menu_pages";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public async Task<IEnumerable<CMSPageDto>> Handle(GetMenuCMSPagesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving menu CMS pages");

        var cachedPages = await cache.GetAsync<IEnumerable<CMSPageDto>>(CACHE_KEY_MENU_PAGES, cancellationToken);
        if (cachedPages != null)
        {
            logger.LogInformation("Cache hit for menu CMS pages");
            return cachedPages;
        }

        logger.LogInformation("Cache miss for menu CMS pages");

        var pages = await context.Set<CMSPage>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .Where(p => p.ShowInMenu && p.Status == ContentStatus.Published && p.ParentPageId == null)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Title)
            .Take(100)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} menu CMS pages", pages.Count);

        var pageDtos = new List<CMSPageDto>(pages.Count);
        foreach (var page in pages)
        {
            pageDtos.Add(mapper.Map<CMSPageDto>(page));
        }

        await cache.SetAsync(CACHE_KEY_MENU_PAGES, pageDtos, CACHE_EXPIRATION, cancellationToken);

        return pageDtos;
    }
}

