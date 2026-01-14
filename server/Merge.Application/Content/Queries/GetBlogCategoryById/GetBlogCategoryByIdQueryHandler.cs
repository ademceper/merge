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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetBlogCategoryByIdQueryHandler : IRequestHandler<GetBlogCategoryByIdQuery, BlogCategoryDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBlogCategoryByIdQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_CATEGORY_BY_ID = "blog_category_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10); // Categories change less frequently

    public GetBlogCategoryByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetBlogCategoryByIdQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<BlogCategoryDto?> Handle(GetBlogCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving blog category with Id: {CategoryId}", request.Id);

        var cacheKey = $"{CACHE_KEY_CATEGORY_BY_ID}{request.Id}";

        // ✅ BOLUM 10.2: Redis distributed cache for single category
        var cachedCategory = await _cache.GetAsync<BlogCategoryDto>(cacheKey, cancellationToken);
        if (cachedCategory != null)
        {
            _logger.LogInformation("Cache hit for blog category. CategoryId: {CategoryId}", request.Id);
            return cachedCategory;
        }

        _logger.LogInformation("Cache miss for blog category. CategoryId: {CategoryId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var category = await _context.Set<BlogCategory>()
            .AsNoTracking()
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            _logger.LogWarning("Blog category not found with Id: {CategoryId}", request.Id);
            return null;
        }

        // Get post count
        var postCount = await _context.Set<BlogPost>()
            .AsNoTracking()
            .CountAsync(p => p.CategoryId == request.Id, cancellationToken);

        _logger.LogInformation("Successfully retrieved blog category {CategoryId}", request.Id);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        // ✅ BOLUM 7.1: Records - with expression kullanımı (immutable DTOs)
        var categoryDto = _mapper.Map<BlogCategoryDto>(category);
        categoryDto = categoryDto with { PostCount = postCount };

        // Cache the result
        await _cache.SetAsync(cacheKey, categoryDto, CACHE_EXPIRATION, cancellationToken);

        return categoryDto;
    }
}

