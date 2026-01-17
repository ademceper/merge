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

namespace Merge.Application.Content.Queries.GetActiveBanners;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetActiveBannersQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetActiveBannersQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetActiveBannersQuery, PagedResult<BannerDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private const string CACHE_KEY_ACTIVE_BANNERS_PAGED = "banners_active_paged";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5); // Active banners change more frequently

    public async Task<PagedResult<BannerDto>> Handle(GetActiveBannersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving active banners. Position: {Position}, Page: {Page}, PageSize: {PageSize}",
            request.Position, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_ACTIVE_BANNERS_PAGED}_{request.Position ?? "all"}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for active banners
        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for active banners (paged). Fetching from database.");

                var now = DateTime.UtcNow;
                var query = context.Set<Banner>()
                    .AsNoTracking()
                    .Where(b => b.IsActive &&
                          (!b.StartDate.HasValue || b.StartDate.Value <= now) &&
                          (!b.EndDate.HasValue || b.EndDate.Value >= now));

                if (!string.IsNullOrEmpty(request.Position))
                {
                    query = query.Where(b => b.Position == request.Position);
                }

                var orderedQuery = query.OrderBy(b => b.SortOrder);
                var totalCount = await orderedQuery.CountAsync(cancellationToken);
                var banners = await orderedQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return new PagedResult<BannerDto>
                {
                    Items = mapper.Map<List<BannerDto>>(banners),
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

