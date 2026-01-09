using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Catalog;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Catalog.Queries.GetCategoryById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_CATEGORY_BY_ID = "category_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromHours(1); // Categories change less frequently

    public GetCategoryByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetCategoryByIdQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving category with Id: {CategoryId}", request.Id);

        var cacheKey = $"{CACHE_KEY_CATEGORY_BY_ID}{request.Id}";

        // ✅ BOLUM 10.2: Redis distributed cache for single category
        var cachedCategory = await _cache.GetAsync<CategoryDto>(cacheKey, cancellationToken);
        if (cachedCategory != null)
        {
            _logger.LogInformation("Cache hit for category. CategoryId: {CategoryId}", request.Id);
            return cachedCategory;
        }

        _logger.LogInformation("Cache miss for category. CategoryId: {CategoryId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var category = await _context.Set<Category>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Category not found with Id: {CategoryId}", request.Id);
            return null;
        }

        _logger.LogInformation("Successfully retrieved category {CategoryId} with Name: {Name}",
            request.Id, category.Name);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var categoryDto = _mapper.Map<CategoryDto>(category);
        
        // Cache the result
        await _cache.SetAsync(cacheKey, categoryDto, CACHE_EXPIRATION, cancellationToken);

        return categoryDto;
    }
}

