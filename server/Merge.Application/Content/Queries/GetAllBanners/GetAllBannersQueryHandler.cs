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

public class GetAllBannersQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAllBannersQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAllBannersQuery, PagedResult<BannerDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private const string CACHE_KEY_ALL_BANNERS_PAGED = "banners_all_paged";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(15);

    public async Task<PagedResult<BannerDto>> Handle(GetAllBannersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all banners. Page: {Page}, PageSize: {PageSize}",
            request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_ALL_BANNERS_PAGED}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for all banners (paged). Fetching from database.");

                var query = context.Set<Banner>()
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

