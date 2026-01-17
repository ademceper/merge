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

namespace Merge.Application.Content.Queries.GetAllPageBuilders;

public class GetAllPageBuildersQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAllPageBuildersQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAllPageBuildersQuery, PagedResult<PageBuilderDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private const string CACHE_KEY_PAGE_BUILDERS = "page_builders_paged_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<PagedResult<PageBuilderDto>> Handle(GetAllPageBuildersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving page builders. Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.Status, request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_PAGE_BUILDERS}{request.Status ?? "all"}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for page builders. Status: {Status}, Page: {Page}, PageSize: {PageSize}",
                    request.Status, page, pageSize);

                IQueryable<PageBuilder> query = context.Set<PageBuilder>()
                    .AsNoTracking()
                    .Include(pb => pb.Author);

                if (!string.IsNullOrEmpty(request.Status))
                {
                    if (Enum.TryParse<ContentStatus>(request.Status, true, out var statusEnum))
                    {
                        query = query.Where(pb => pb.Status == statusEnum);
                    }
                }

                var orderedQuery = query.OrderByDescending(pb => pb.CreatedAt);
                var totalCount = await orderedQuery.CountAsync(cancellationToken);

                var pageBuilders = await orderedQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                var items = pageBuilders.Select(pb => mapper.Map<PageBuilderDto>(pb)).ToList();

                return new PagedResult<PageBuilderDto>
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
            return new PagedResult<PageBuilderDto>
            {
                Items = new List<PageBuilderDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        logger.LogInformation("Successfully retrieved {Count} page builders (Page: {Page}, PageSize: {PageSize})",
            cachedResult.Items.Count, page, pageSize);

        return cachedResult;
    }
}

