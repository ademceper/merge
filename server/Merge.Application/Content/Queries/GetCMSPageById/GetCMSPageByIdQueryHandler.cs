using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetCMSPageById;

public class GetCMSPageByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetCMSPageByIdQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetCMSPageByIdQuery, CMSPageDto?>
{
    private const string CACHE_KEY_CMS_PAGE_BY_ID = "cms_page_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public async Task<CMSPageDto?> Handle(GetCMSPageByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving CMS page with Id: {PageId}", request.Id);

        var cacheKey = $"{CACHE_KEY_CMS_PAGE_BY_ID}{request.Id}";

        var cachedPage = await cache.GetAsync<CMSPageDto>(cacheKey, cancellationToken);
        if (cachedPage is not null)
        {
            logger.LogInformation("Cache hit for CMS page. PageId: {PageId}", request.Id);
            return cachedPage;
        }

        logger.LogInformation("Cache miss for CMS page. PageId: {PageId}", request.Id);

        var page = await context.Set<CMSPage>()
            .AsNoTracking()
            .Include(p => p.Author)
            .Include(p => p.ParentPage)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (page is null)
        {
            logger.LogWarning("CMS page not found with Id: {PageId}", request.Id);
            return null;
        }

        logger.LogInformation("Successfully retrieved CMS page {PageId} with Title: {Title}",
            request.Id, page.Title);

        var pageDto = mapper.Map<CMSPageDto>(page);
        
        await cache.SetAsync(cacheKey, pageDto, CACHE_EXPIRATION, cancellationToken);

        return pageDto;
    }
}

