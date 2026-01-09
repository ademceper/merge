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

namespace Merge.Application.Content.Queries.GetAllCMSPages;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAllCMSPagesQueryHandler : IRequestHandler<GetAllCMSPagesQuery, PagedResult<CMSPageDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllCMSPagesQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_ALL_PAGES_PAGED = "cms_pages_all_paged";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public GetAllCMSPagesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllCMSPagesQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<CMSPageDto>> Handle(GetAllCMSPagesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all CMS pages. Status: {Status}, ShowInMenu: {ShowInMenu}, Page: {Page}, PageSize: {PageSize}",
            request.Status, request.ShowInMenu, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_ALL_PAGES_PAGED}_{request.Status ?? "all"}_{request.ShowInMenu?.ToString() ?? "all"}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for paginated queries
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for all CMS pages (paged). Fetching from database.");

                IQueryable<CMSPage> query = _context.Set<CMSPage>()
                    .AsNoTracking()
                    .Include(p => p.Author)
                    .Include(p => p.ParentPage);

                if (!string.IsNullOrEmpty(request.Status))
                {
                    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
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
                    Items = _mapper.Map<List<CMSPageDto>>(pages),
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

