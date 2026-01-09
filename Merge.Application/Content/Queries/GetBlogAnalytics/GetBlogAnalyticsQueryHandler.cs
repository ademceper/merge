using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Queries.GetBlogAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetBlogAnalyticsQueryHandler : IRequestHandler<GetBlogAnalyticsQuery, BlogAnalyticsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetBlogAnalyticsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_BLOG_ANALYTICS = "blog_analytics_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10);

    public GetBlogAnalyticsQueryHandler(
        IDbContext context,
        ILogger<GetBlogAnalyticsQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<BlogAnalyticsDto> Handle(GetBlogAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving blog analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        var start = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var end = request.EndDate ?? DateTime.UtcNow;

        var cacheKey = $"{CACHE_KEY_BLOG_ANALYTICS}{start:yyyy-MM-dd}_{end:yyyy-MM-dd}";

        // ✅ BOLUM 10.2: Redis distributed cache
        var cachedAnalytics = await _cache.GetAsync<BlogAnalyticsDto>(cacheKey, cancellationToken);
        if (cachedAnalytics != null)
        {
            _logger.LogInformation("Cache hit for blog analytics. StartDate: {StartDate}, EndDate: {EndDate}",
                request.StartDate, request.EndDate);
            return cachedAnalytics;
        }

        _logger.LogInformation("Cache miss for blog analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var query = _context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end);

        var totalPosts = await query.CountAsync(cancellationToken);
        var publishedPosts = await query.CountAsync(p => p.Status == ContentStatus.Published, cancellationToken);
        var draftPosts = await query.CountAsync(p => p.Status == ContentStatus.Draft, cancellationToken);
        var totalViews = await query.SumAsync(p => (long)p.ViewCount, cancellationToken);
        var totalComments = await query.SumAsync(p => (long)p.CommentCount, cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap
        var postsByCategory = await query
            .GroupBy(p => p.Category != null ? p.Category.Name : "Uncategorized")
            .Select(g => new { CategoryName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CategoryName, g => g.Count, cancellationToken);

        // ✅ PERFORMANCE: Database'de filtering, ordering ve projection yap
        var popularPosts = await query
            .Where(p => p.Status == ContentStatus.Published)
            .OrderByDescending(p => p.ViewCount)
            .Take(10)
            .Select(p => new PopularPostDto(
                p.Id,
                p.Title,
                p.ViewCount,
                p.CommentCount))
            .ToListAsync(cancellationToken);

        var analytics = new BlogAnalyticsDto(
            totalPosts,
            publishedPosts,
            draftPosts,
            (int)totalViews,
            (int)totalComments,
            postsByCategory,
            popularPosts
        );

        _logger.LogInformation("Successfully retrieved blog analytics. TotalPosts: {TotalPosts}, PublishedPosts: {PublishedPosts}",
            totalPosts, publishedPosts);

        // ✅ BOLUM 10.2: Cache the result
        await _cache.SetAsync(cacheKey, analytics, CACHE_EXPIRATION, cancellationToken);

        return analytics;
    }
}

