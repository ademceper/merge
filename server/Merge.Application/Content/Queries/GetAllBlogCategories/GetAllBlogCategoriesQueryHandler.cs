using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetAllBlogCategories;

public class GetAllBlogCategoriesQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAllBlogCategoriesQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetAllBlogCategoriesQuery, IEnumerable<BlogCategoryDto>>
{
    private const string CACHE_KEY_ALL_CATEGORIES = "blog_categories_all";
    private const string CACHE_KEY_ACTIVE_CATEGORIES = "blog_categories_active";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10);

    public async Task<IEnumerable<BlogCategoryDto>> Handle(GetAllBlogCategoriesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving all blog categories. IsActive: {IsActive}", request.IsActive);

        var cacheKey = request.IsActive == true ? CACHE_KEY_ACTIVE_CATEGORIES : CACHE_KEY_ALL_CATEGORIES;

        var cachedCategories = await cache.GetAsync<List<BlogCategoryDto>>(cacheKey, cancellationToken);
        if (cachedCategories != null)
        {
            logger.LogInformation("Cache hit for all blog categories. IsActive: {IsActive}", request.IsActive);
            return cachedCategories;
        }

        logger.LogInformation("Cache miss for all blog categories. IsActive: {IsActive}", request.IsActive);

        IQueryable<BlogCategory> query = context.Set<BlogCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory);

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Take(200)
            .ToListAsync(cancellationToken);

        var categoryIdsSubquery = from c in query.Take(200) select c.Id;
        var postCounts = await context.Set<BlogPost>()
            .AsNoTracking()
            .Where(p => categoryIdsSubquery.Contains(p.CategoryId) && p.Status == ContentStatus.Published)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

        logger.LogInformation("Retrieved {Count} blog categories", categories.Count);

        var result = new List<BlogCategoryDto>(categories.Count);
        foreach (var category in categories)
        {
            var categoryDto = mapper.Map<BlogCategoryDto>(category);
            categoryDto = categoryDto with { PostCount = postCounts.GetValueOrDefault(category.Id, 0) };
            result.Add(categoryDto);
        }

        await cache.SetAsync(cacheKey, result, CACHE_EXPIRATION, cancellationToken);

        return result;
    }
}

