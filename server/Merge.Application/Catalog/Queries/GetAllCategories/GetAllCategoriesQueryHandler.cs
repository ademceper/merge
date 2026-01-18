using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Catalog;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Catalog.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAllCategoriesQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAllCategoriesQuery, PagedResult<CategoryDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private const string CACHE_KEY_ALL_CATEGORIES_PAGED = "categories_all_paged";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromHours(1);

    public async Task<PagedResult<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all categories. Page: {Page}, PageSize: {PageSize}",
            request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_ALL_CATEGORIES_PAGED}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for all categories (paged). Fetching from database.");

                var query = context.Set<Category>()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(c => c.ParentCategory)
                    .Include(c => c.SubCategories); // Include subcategories for mapping

                var totalCount = await query.CountAsync(cancellationToken);
                var categories = await query
                    .OrderBy(c => c.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                return new PagedResult<CategoryDto>
                {
                    Items = mapper.Map<List<CategoryDto>>(categories),
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

