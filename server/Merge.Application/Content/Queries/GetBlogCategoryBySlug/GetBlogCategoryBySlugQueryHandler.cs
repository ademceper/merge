using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetBlogCategoryBySlug;

public class GetBlogCategoryBySlugQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetBlogCategoryBySlugQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetBlogCategoryBySlugQuery, BlogCategoryDto?>
{
    private const string CACHE_KEY_CATEGORY_BY_SLUG = "blog_category_slug_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10);

    public async Task<BlogCategoryDto?> Handle(GetBlogCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving blog category with Slug: {Slug}", request.Slug);

        var cacheKey = $"{CACHE_KEY_CATEGORY_BY_SLUG}{request.Slug}";

        var cachedCategory = await cache.GetAsync<BlogCategoryDto>(cacheKey, cancellationToken);
        if (cachedCategory != null)
        {
            logger.LogInformation("Cache hit for blog category. Slug: {Slug}", request.Slug);
            return cachedCategory;
        }

        logger.LogInformation("Cache miss for blog category. Slug: {Slug}", request.Slug);

        var category = await context.Set<BlogCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Slug == request.Slug && c.IsActive, cancellationToken);

        if (category == null)
        {
            logger.LogWarning("Blog category not found with Slug: {Slug}", request.Slug);
            return null;
        }

        var postCount = await context.Set<BlogPost>()
            .AsNoTracking()
            .CountAsync(p => p.CategoryId == category.Id, cancellationToken);

        logger.LogInformation("Successfully retrieved blog category {CategoryId} by slug {Slug}", category.Id, request.Slug);

        var categoryDto = mapper.Map<BlogCategoryDto>(category);
        categoryDto = categoryDto with { PostCount = postCount };

        await cache.SetAsync(cacheKey, categoryDto, CACHE_EXPIRATION, cancellationToken);

        return categoryDto;
    }
}

