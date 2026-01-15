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

public class GetFeaturedBlogPostsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetFeaturedBlogPostsQueryHandler> logger,
    ICacheService cache,
    IOptions<ContentSettings> contentSettings) : IRequestHandler<GetFeaturedBlogPostsQuery, IEnumerable<BlogPostDto>>
{
    private const string CACHE_KEY_FEATURED_POSTS = "blog_posts_featured_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(10);

    public async Task<IEnumerable<BlogPostDto>> Handle(GetFeaturedBlogPostsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving featured blog posts. Count: {Count}", request.Count);

        var count = request.Count > contentSettings.Value.MaxFeaturedPostsCount ? contentSettings.Value.MaxFeaturedPostsCount : request.Count;

        var cacheKey = $"{CACHE_KEY_FEATURED_POSTS}{count}";

        var cachedPosts = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for featured blog posts. Count: {Count}", count);

                var posts = await context.Set<BlogPost>()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(p => p.Category)
                    .Include(p => p.Author)
                    .Where(p => p.IsFeatured && p.Status == ContentStatus.Published)
                    .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                    .Take(count)
                    .ToListAsync(cancellationToken);

                logger.LogInformation("Retrieved {Count} featured blog posts", posts.Count);

                return mapper.Map<List<BlogPostDto>>(posts);
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedPosts!;
    }
}

