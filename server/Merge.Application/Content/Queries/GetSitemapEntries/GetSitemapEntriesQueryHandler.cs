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
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetSitemapEntries;

public class GetSitemapEntriesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetSitemapEntriesQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetSitemapEntriesQuery, PagedResult<SitemapEntryDto>>
{
    private const string CACHE_KEY_SITEMAP_ENTRIES = "sitemap_entries_paged_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public async Task<PagedResult<SitemapEntryDto>> Handle(GetSitemapEntriesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving sitemap entries. IsActive: {IsActive}, Page: {Page}, PageSize: {PageSize}",
            request.IsActive, request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationSettings.Value.MaxPageSize ? paginationSettings.Value.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_SITEMAP_ENTRIES}{request.IsActive?.ToString() ?? "all"}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for sitemap entries. IsActive: {IsActive}, Page: {Page}, PageSize: {PageSize}",
                    request.IsActive, page, pageSize);

                IQueryable<SitemapEntry> query = context.Set<SitemapEntry>()
                    .AsNoTracking();

                if (request.IsActive.HasValue)
                {
                    query = query.Where(e => e.IsActive == request.IsActive.Value);
                }

                var orderedQuery = query.OrderBy(e => e.PageType).ThenBy(e => e.Priority);
                var totalCount = await orderedQuery.CountAsync(cancellationToken);

                var entries = await orderedQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Retrieved {Count} sitemap entries (page {Page})", entries.Count, page);

                return new PagedResult<SitemapEntryDto>
                {
                    Items = mapper.Map<List<SitemapEntryDto>>(entries),
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

