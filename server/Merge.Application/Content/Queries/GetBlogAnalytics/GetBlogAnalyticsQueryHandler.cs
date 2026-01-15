using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetBlogAnalytics;

public class GetBlogAnalyticsQueryHandler(
    IDbContext context,
    ILogger<GetBlogAnalyticsQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetBlogAnalyticsQuery, BlogAnalyticsDto>
{
    private const string CACHE_KEY_BLOG_ANALYTICS = "blog_analytics_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10);

    public async Task<BlogAnalyticsDto> Handle(GetBlogAnalyticsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving blog analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        var start = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var end = request.EndDate ?? DateTime.UtcNow;

        var cacheKey = $"{CACHE_KEY_BLOG_ANALYTICS}{start:yyyy-MM-dd}_{end:yyyy-MM-dd}";

        var cachedAnalytics = await cache.GetAsync<BlogAnalyticsDto>(cacheKey, cancellationToken);
        if (cachedAnalytics != null)
        {
            logger.LogInformation("Cache hit for blog analytics. StartDate: {StartDate}, EndDate: {EndDate}",
                request.StartDate, request.EndDate);
            return cachedAnalytics;
        }

        logger.LogInformation("Cache miss for blog analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        var query = context.Set<BlogPost>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CreatedAt >= start && p.CreatedAt <= end);

        var totalPosts = await query.CountAsync(cancellationToken);
        var publishedPosts = await query.CountAsync(p => p.Status == ContentStatus.Published, cancellationToken);
        var draftPosts = await query.CountAsync(p => p.Status == ContentStatus.Draft, cancellationToken);
        var totalViews = await query.SumAsync(p => (long)p.ViewCount, cancellationToken);
        var totalComments = await query.SumAsync(p => (long)p.CommentCount, cancellationToken);

        var postsByCategory = await query
            .GroupBy(p => p.Category != null ? p.Category.Name : "Uncategorized")
            .Select(g => new { CategoryName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CategoryName, g => g.Count, cancellationToken);

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

        logger.LogInformation("Successfully retrieved blog analytics. TotalPosts: {TotalPosts}, PublishedPosts: {PublishedPosts}",
            totalPosts, publishedPosts);

        await cache.SetAsync(cacheKey, analytics, CACHE_EXPIRATION, cancellationToken);

        return analytics;
    }
}

