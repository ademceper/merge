using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetAllCMSPages;

public class GetAllCMSPagesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAllCMSPagesQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAllCMSPagesQuery, PagedResult<CMSPageDto>>
{
    private const string CACHE_KEY_ALL_PAGES_PAGED = "cms_pages_all_paged";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public async Task<PagedResult<CMSPageDto>> Handle(GetAllCMSPagesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all CMS pages. Status: {Status}, ShowInMenu: {ShowInMenu}, Page: {Page}, PageSize: {PageSize}",
            request.Status, request.ShowInMenu, request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationSettings.Value.MaxPageSize ? paginationSettings.Value.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_ALL_PAGES_PAGED}_{request.Status ?? "all"}_{request.ShowInMenu?.ToString() ?? "all"}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for all CMS pages (paged). Fetching from database.");

                IQueryable<CMSPage> query = context.Set<CMSPage>()
                    .AsNoTracking()
                    .Include(p => p.Author)
                    .Include(p => p.ParentPage);

                if (!string.IsNullOrEmpty(request.Status))
                {
                    if (Enum.TryParse<ContentStatus>(request.Status, true, out var statusEnum))
                    {
                        query = query.Where(p => p.Status == statusEnum);
                    }
                }

                if (request.ShowInMenu.HasValue)
                {
                    query = query.Where(p => p.ShowInMenu == request.ShowInMenu.Value);
                }

                var totalCount = await query.CountAsync(cancellationToken);
                var pages = await query
                    .OrderBy(p => p.DisplayOrder)
                    .ThenBy(p => p.Title)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return new PagedResult<CMSPageDto>
                {
                    Items = mapper.Map<List<CMSPageDto>>(pages),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedResult!;
    }
}

