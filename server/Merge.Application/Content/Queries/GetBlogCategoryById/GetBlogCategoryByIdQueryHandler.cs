using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetBlogCategoryById;

public class GetBlogCategoryByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetBlogCategoryByIdQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetBlogCategoryByIdQuery, BlogCategoryDto?>
{
    private const string CACHE_KEY_CATEGORY_BY_ID = "blog_category_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10);

    public async Task<BlogCategoryDto?> Handle(GetBlogCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving blog category with Id: {CategoryId}", request.Id);

        var cacheKey = $"{CACHE_KEY_CATEGORY_BY_ID}{request.Id}";

        var cachedCategory = await cache.GetAsync<BlogCategoryDto>(cacheKey, cancellationToken);
        if (cachedCategory is not null)
        {
            logger.LogInformation("Cache hit for blog category. CategoryId: {CategoryId}", request.Id);
            return cachedCategory;
        }

        logger.LogInformation("Cache miss for blog category. CategoryId: {CategoryId}", request.Id);

        var category = await context.Set<BlogCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category is null)
        {
            logger.LogWarning("Blog category not found with Id: {CategoryId}", request.Id);
            return null;
        }

        var postCount = await context.Set<BlogPost>()
            .AsNoTracking()
            .CountAsync(p => p.CategoryId == request.Id, cancellationToken);

        logger.LogInformation("Successfully retrieved blog category {CategoryId}", request.Id);

        var categoryDto = mapper.Map<BlogCategoryDto>(category);
        categoryDto = categoryDto with { PostCount = postCount };

        await cache.SetAsync(cacheKey, categoryDto, CACHE_EXPIRATION, cancellationToken);

        return categoryDto;
    }
}

