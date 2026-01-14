using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetFeaturedBlogPosts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetFeaturedBlogPostsQueryHandler : IRequestHandler<GetFeaturedBlogPostsQuery, IEnumerable<BlogPostDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFeaturedBlogPostsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly ContentSettings _contentSettings;
    private const string CACHE_KEY_FEATURED_POSTS = "blog_posts_featured_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10); // Featured posts change less frequently

    public GetFeaturedBlogPostsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetFeaturedBlogPostsQueryHandler> logger,
        ICacheService cache,
        IOptions<ContentSettings> contentSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _contentSettings = contentSettings.Value;
    }

    public async Task<IEnumerable<BlogPostDto>> Handle(GetFeaturedBlogPostsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving featured blog posts. Count: {Count}", request.Count);

        // ✅ BOLUM 6.3: Unbounded Query Koruması - Configuration'dan max limit
        var count = request.Count > _contentSettings.MaxFeaturedPostsCount ? _contentSettings.MaxFeaturedPostsCount : request.Count;

        var cacheKey = $"{CACHE_KEY_FEATURED_POSTS}{count}";

        // ✅ BOLUM 10.2: Redis distributed cache for featured blog posts
        var cachedPosts = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for featured blog posts. Count: {Count}", count);

                // ✅ PERFORMANCE: AsNoTracking for read-only queries
                var posts = await _context.Set<BlogPost>()
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Author)
                    .Where(p => p.IsFeatured && p.Status == ContentStatus.Published)
                    .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                    .Take(count)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} featured blog posts", posts.Count);

                return _mapper.Map<List<BlogPostDto>>(posts);
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedPosts!;
    }
}

