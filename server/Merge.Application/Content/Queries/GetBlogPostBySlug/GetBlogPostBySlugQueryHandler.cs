using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetBlogPostBySlug;

public class GetBlogPostBySlugQueryHandler(
    IDbContext context,
    Merge.Application.Interfaces.IRepository<BlogPost> postRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<GetBlogPostBySlugQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetBlogPostBySlugQuery, BlogPostDto?>
{
    private const string CACHE_KEY_POST_BY_SLUG = "blog_post_slug_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<BlogPostDto?> Handle(GetBlogPostBySlugQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving blog post with Slug: {Slug}, TrackView: {TrackView}", request.Slug, request.TrackView);

        var cacheKey = $"{CACHE_KEY_POST_BY_SLUG}{request.Slug}";

        var cachedPost = await cache.GetAsync<BlogPostDto>(cacheKey, cancellationToken);
        if (cachedPost != null && !request.TrackView)
        {
            logger.LogInformation("Cache hit for blog post. Slug: {Slug}", request.Slug);
            return cachedPost;
        }

        logger.LogInformation("Cache miss for blog post. Slug: {Slug}", request.Slug);

        var post = request.TrackView
            ? await context.Set<BlogPost>()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Slug == request.Slug && p.Status == ContentStatus.Published, cancellationToken)
            : await context.Set<BlogPost>()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Slug == request.Slug && p.Status == ContentStatus.Published, cancellationToken);

        if (post == null)
        {
            logger.LogWarning("Blog post not found with Slug: {Slug}", request.Slug);
            return null;
        }

        if (request.TrackView)
        {
            post.IncrementViewCount();
            await postRepository.UpdateAsync(post, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Successfully retrieved blog post {PostId} by slug {Slug}", post.Id, request.Slug);

        var postDto = mapper.Map<BlogPostDto>(post);

        if (!request.TrackView)
        {
            await cache.SetAsync(cacheKey, postDto, CACHE_EXPIRATION, cancellationToken);
        }

        return postDto;
    }
}

