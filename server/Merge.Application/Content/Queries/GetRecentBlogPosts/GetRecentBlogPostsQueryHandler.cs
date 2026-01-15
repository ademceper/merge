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

namespace Merge.Application.Content.Queries.GetRecentBlogPosts;

public class GetRecentBlogPostsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetRecentBlogPostsQueryHandler> logger,
    ICacheService cache,
    IOptions<ContentSettings> contentSettings) : IRequestHandler<GetRecentBlogPostsQuery, IEnumerable<BlogPostDto>>
{
    private const string CACHE_KEY_RECENT_POSTS = "blog_posts_recent_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<IEnumerable<BlogPostDto>> Handle(GetRecentBlogPostsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving recent blog posts. Count: {Count}", request.Count);

        var count = request.Count > contentSettings.Value.MaxRecentPostsCount ? contentSettings.Value.MaxRecentPostsCount : request.Count;

        var cacheKey = $"{CACHE_KEY_RECENT_POSTS}{count}";

        var cachedPosts = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for recent blog posts. Count: {Count}", count);

                var posts = await context.Set<BlogPost>()
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Author)
                    .Where(p => p.Status == ContentStatus.Published)
                    .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                    .Take(count)
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Retrieved {Count} recent blog posts", posts.Count);

                return mapper.Map<List<BlogPostDto>>(posts);
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedPosts!;
    }
}

