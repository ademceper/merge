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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetSitemapEntriesQueryHandler : IRequestHandler<GetSitemapEntriesQuery, PagedResult<SitemapEntryDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSitemapEntriesQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_SITEMAP_ENTRIES = "sitemap_entries_paged_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15); // Sitemap entries change less frequently

    public GetSitemapEntriesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSitemapEntriesQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<SitemapEntryDto>> Handle(GetSitemapEntriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving sitemap entries. IsActive: {IsActive}, Page: {Page}, PageSize: {PageSize}",
            request.IsActive, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_SITEMAP_ENTRIES}{request.IsActive?.ToString() ?? "all"}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for paginated sitemap entries
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for sitemap entries. IsActive: {IsActive}, Page: {Page}, PageSize: {PageSize}",
                    request.IsActive, page, pageSize);

                // ✅ PERFORMANCE: AsNoTracking for read-only queries
                IQueryable<SitemapEntry> query = _context.Set<SitemapEntry>()
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

                _logger.LogInformation("Retrieved {Count} sitemap entries (page {Page})", entries.Count, page);

                return new PagedResult<SitemapEntryDto>
                {
                    Items = _mapper.Map<List<SitemapEntryDto>>(entries),
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

