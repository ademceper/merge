using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Queries.GetAllBlogCategories;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAllBlogCategoriesQueryHandler : IRequestHandler<GetAllBlogCategoriesQuery, IEnumerable<BlogCategoryDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllBlogCategoriesQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_ALL_CATEGORIES = "blog_categories_all";
    private const string CACHE_KEY_ACTIVE_CATEGORIES = "blog_categories_active";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10); // Categories change less frequently

    public GetAllBlogCategoriesQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllBlogCategoriesQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<BlogCategoryDto>> Handle(GetAllBlogCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving all blog categories. IsActive: {IsActive}", request.IsActive);

        var cacheKey = request.IsActive == true ? CACHE_KEY_ACTIVE_CATEGORIES : CACHE_KEY_ALL_CATEGORIES;

        // ✅ BOLUM 10.2: Redis distributed cache for all categories
        var cachedCategories = await _cache.GetAsync<List<BlogCategoryDto>>(cacheKey, cancellationToken);
        if (cachedCategories != null)
        {
            _logger.LogInformation("Cache hit for all blog categories. IsActive: {IsActive}", request.IsActive);
            return cachedCategories;
        }

        _logger.LogInformation("Cache miss for all blog categories. IsActive: {IsActive}", request.IsActive);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        IQueryable<BlogCategory> query = _context.Set<BlogCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory);

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        // ✅ BOLUM 6.3: Unbounded Query Koruması - Kategoriler genelde sınırlı ama güvenlik için limit ekle
        var categories = await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Take(200) // ✅ Güvenlik: Maksimum 200 kategori
            .ToListAsync(cancellationToken);

        // Get post counts
        var categoryIds = categories.Select(c => c.Id).ToList();
        var postCounts = await _context.Set<BlogPost>()
            .AsNoTracking()
            .Where(p => categoryIds.Contains(p.CategoryId) && p.Status == ContentStatus.Published)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

        _logger.LogInformation("Retrieved {Count} blog categories", categories.Count);

        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        // ✅ BOLUM 7.1: Records - with expression kullanımı (immutable DTOs)
        var result = new List<BlogCategoryDto>(categories.Count);
        foreach (var category in categories)
        {
            var categoryDto = _mapper.Map<BlogCategoryDto>(category);
            // ✅ BOLUM 7.1: Records - with expression ile computed property güncelleme
            categoryDto = categoryDto with { PostCount = postCounts.GetValueOrDefault(category.Id, 0) };
            result.Add(categoryDto);
        }

        // Cache the result
        await _cache.SetAsync(cacheKey, result, CACHE_EXPIRATION, cancellationToken);

        return result;
    }
}

