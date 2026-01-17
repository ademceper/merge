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

namespace Merge.Application.Catalog.Queries.GetMainCategories;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetMainCategoriesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetMainCategoriesQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetMainCategoriesQuery, PagedResult<CategoryDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private const string CACHE_KEY_MAIN_CATEGORIES_PAGED = "categories_main_paged";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromHours(1);

    public async Task<PagedResult<CategoryDto>> Handle(GetMainCategoriesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving main categories. Page: {Page}, PageSize: {PageSize}",
            request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_MAIN_CATEGORIES_PAGED}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for frequently accessed, rarely changed data
        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for main categories (paged). Fetching from database.");

                var query = context.Set<Category>()
                    .AsNoTracking()
                    .Include(c => c.SubCategories)
                    .Where(c => c.ParentCategoryId == null);

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

