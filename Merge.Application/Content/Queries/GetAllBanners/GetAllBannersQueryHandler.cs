using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetAllBanners;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAllBannersQueryHandler : IRequestHandler<GetAllBannersQuery, PagedResult<BannerDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllBannersQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_ALL_BANNERS_PAGED = "banners_all_paged";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public GetAllBannersQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllBannersQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<BannerDto>> Handle(GetAllBannersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all banners. Page: {Page}, PageSize: {PageSize}",
            request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_ALL_BANNERS_PAGED}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for paginated queries
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for all banners (paged). Fetching from database.");

                var query = _context.Set<Banner>()
                    .AsNoTracking()
                    .OrderBy(b => b.Position)
                    .ThenBy(b => b.SortOrder);

                var totalCount = await query.CountAsync(cancellationToken);
                var banners = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return new PagedResult<BannerDto>
                {
                    Items = _mapper.Map<List<BannerDto>>(banners),
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

