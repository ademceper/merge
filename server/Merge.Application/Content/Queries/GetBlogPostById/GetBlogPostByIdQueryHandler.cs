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
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.BlogPost>;

namespace Merge.Application.Content.Queries.GetBlogPostById;

public class GetBlogPostByIdQueryHandler(
    IDbContext context,
    IRepository postRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<GetBlogPostByIdQueryHandler> logger,
    ICacheService cache) : IRequestHandler<GetBlogPostByIdQuery, BlogPostDto?>
{
    private const string CACHE_KEY_POST_BY_ID = "blog_post_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5); // Blog posts change frequently

    public async Task<BlogPostDto?> Handle(GetBlogPostByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving blog post with Id: {PostId}, TrackView: {TrackView}", request.Id, request.TrackView);

        var cacheKey = $"{CACHE_KEY_POST_BY_ID}{request.Id}";

        var cachedPost = await cache.GetAsync<BlogPostDto>(cacheKey, cancellationToken);
        if (cachedPost != null && !request.TrackView) // Don't use cache if tracking view (need to update)
        {
            logger.LogInformation("Cache hit for blog post. PostId: {PostId}", request.Id);
            return cachedPost;
        }

        logger.LogInformation("Cache miss for blog post. PostId: {PostId}", request.Id);

        var post = request.TrackView
            ? await context.Set<BlogPost>()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            : await context.Set<BlogPost>()
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (post == null)
        {
            logger.LogWarning("Blog post not found with Id: {PostId}", request.Id);
            return null;
        }

        if (request.TrackView && post.Status == ContentStatus.Published)
        {
            post.IncrementViewCount();
            await postRepository.UpdateAsync(post, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        logger.LogInformation("Successfully retrieved blog post {PostId}", request.Id);

        var postDto = mapper.Map<BlogPostDto>(post);

        // Cache the result (only if not tracking view)
        if (!request.TrackView)
        {
            await cache.SetAsync(cacheKey, postDto, CACHE_EXPIRATION, cancellationToken);
        }

        return postDto;
    }
}

