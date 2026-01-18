using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Catalog;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Catalog.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetCategoryByIdQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private const string CACHE_KEY_CATEGORY_BY_ID = "category_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromHours(1); // Categories change less frequently

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving category with Id: {CategoryId}", request.Id);

        var cacheKey = $"{CACHE_KEY_CATEGORY_BY_ID}{request.Id}";

        var cachedCategory = await cache.GetAsync<CategoryDto>(cacheKey, cancellationToken);
        if (cachedCategory != null)
        {
            logger.LogInformation("Cache hit for category. CategoryId: {CategoryId}", request.Id);
            return cachedCategory;
        }

        logger.LogInformation("Cache miss for category. CategoryId: {CategoryId}", request.Id);

        var category = await context.Set<Category>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            logger.LogWarning("Category not found with Id: {CategoryId}", request.Id);
            return null;
        }

        logger.LogInformation("Successfully retrieved category {CategoryId} with Name: {Name}",
            request.Id, category.Name);

        var categoryDto = mapper.Map<CategoryDto>(category);
        
        // Cache the result
        await cache.SetAsync(cacheKey, categoryDto, CACHE_EXPIRATION, cancellationToken);

        return categoryDto;
    }
}

