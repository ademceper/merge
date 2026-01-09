using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Queries.GetBlogCategoryBySlug;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetBlogCategoryBySlugQueryHandler : IRequestHandler<GetBlogCategoryBySlugQuery, BlogCategoryDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBlogCategoryBySlugQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_CATEGORY_BY_SLUG = "blog_category_slug_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10); // Categories change less frequently

    public GetBlogCategoryBySlugQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetBlogCategoryBySlugQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<BlogCategoryDto?> Handle(GetBlogCategoryBySlugQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving blog category with Slug: {Slug}", request.Slug);

        var cacheKey = $"{CACHE_KEY_CATEGORY_BY_SLUG}{request.Slug}";

        // ✅ BOLUM 10.2: Redis distributed cache for single category by slug
        var cachedCategory = await _cache.GetAsync<BlogCategoryDto>(cacheKey, cancellationToken);
        if (cachedCategory != null)
        {
            _logger.LogInformation("Cache hit for blog category. Slug: {Slug}", request.Slug);
            return cachedCategory;
        }

        _logger.LogInformation("Cache miss for blog category. Slug: {Slug}", request.Slug);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var category = await _context.Set<BlogCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Slug == request.Slug && c.IsActive, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Blog category not found with Slug: {Slug}", request.Slug);
            return null;
        }

        // Get post count
        var postCount = await _context.Set<BlogPost>()
            .AsNoTracking()
            .CountAsync(p => p.CategoryId == category.Id, cancellationToken);

        _logger.LogInformation("Successfully retrieved blog category {CategoryId} by slug {Slug}", category.Id, request.Slug);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        // ✅ BOLUM 7.1: Records - with expression kullanımı (immutable DTOs)
        var categoryDto = _mapper.Map<BlogCategoryDto>(category);
        categoryDto = categoryDto with { PostCount = postCount };

        // Cache the result
        await _cache.SetAsync(cacheKey, categoryDto, CACHE_EXPIRATION, cancellationToken);

        return categoryDto;
    }
}

