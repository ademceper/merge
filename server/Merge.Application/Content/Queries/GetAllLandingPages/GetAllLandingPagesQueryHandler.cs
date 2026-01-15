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

namespace Merge.Application.Content.Queries.GetAllLandingPages;

public class GetAllLandingPagesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAllLandingPagesQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAllLandingPagesQuery, PagedResult<LandingPageDto>>
{
    private const string CACHE_KEY_LANDING_PAGES = "landing_pages_paged_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<PagedResult<LandingPageDto>> Handle(GetAllLandingPagesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving landing pages. Status: {Status}, IsActive: {IsActive}, Page: {Page}, PageSize: {PageSize}",
            request.Status, request.IsActive, request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationSettings.Value.MaxPageSize ? paginationSettings.Value.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_LANDING_PAGES}{request.Status ?? "all"}_{request.IsActive?.ToString() ?? "all"}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for landing pages. Status: {Status}, IsActive: {IsActive}, Page: {Page}, PageSize: {PageSize}",
                    request.Status, request.IsActive, page, pageSize);

                IQueryable<LandingPage> query = context.Set<LandingPage>()
                    .AsNoTracking()
                    .Include(lp => lp.Author)
                    .Where(lp => lp.VariantOfId == null);

                if (!string.IsNullOrEmpty(request.Status))
                {
                    if (Enum.TryParse<ContentStatus>(request.Status, true, out var statusEnum))
                    {
                        query = query.Where(lp => lp.Status == statusEnum);
                    }
                }

                if (request.IsActive.HasValue)
                {
                    query = query.Where(lp => lp.IsActive == request.IsActive.Value);
                }

                var orderedQuery = query.OrderByDescending(lp => lp.CreatedAt);
                var totalCount = await orderedQuery.CountAsync(cancellationToken);

                var landingPages = await orderedQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var items = landingPages.Select(lp => mapper.Map<LandingPageDto>(lp)).ToList();

                return new PagedResult<LandingPageDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            CACHE_EXPIRATION,
            cancellationToken);

        if (cachedResult == null)
        {
            return new PagedResult<LandingPageDto>
            {
                Items = new List<LandingPageDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        logger.LogInformation("Successfully retrieved {Count} landing pages (Page: {Page}, PageSize: {PageSize})",
            cachedResult.Items.Count, page, pageSize);

        return cachedResult;
    }
}

