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

namespace Merge.Application.Content.Queries.GetHomePageCMSPage;

public class GetHomePageCMSPageQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetHomePageCMSPageQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetHomePageCMSPageQuery, CMSPageDto?>
{
    private const string CACHE_KEY_HOME_PAGE = "cms_home_page";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public async Task<CMSPageDto?> Handle(GetHomePageCMSPageQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving home page CMS page");

        var cachedPage = await cache.GetAsync<CMSPageDto>(CACHE_KEY_HOME_PAGE, cancellationToken);
        if (cachedPage is not null)
        {
            logger.LogInformation("Cache hit for home page");
            return cachedPage;
        }

        logger.LogInformation("Cache miss for home page");

        var page = await context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.IsHomePage && p.Status == ContentStatus.Published, cancellationToken);

        if (page is null)
        {
            logger.LogWarning("Home page not found");
            return null;
        }

        logger.LogInformation("Successfully retrieved home page {PageId} with Title: {Title}",
            page.Id, page.Title);

        var pageDto = mapper.Map<CMSPageDto>(page);
        
        await cache.SetAsync(CACHE_KEY_HOME_PAGE, pageDto, CACHE_EXPIRATION, cancellationToken);

        return pageDto;
    }
}

