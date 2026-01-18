using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.BlogPost>;

namespace Merge.Application.Content.Commands.UpdateBlogPost;

public class UpdateBlogPostCommandHandler(
    IRepository postRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<UpdateBlogPostCommandHandler> logger) : IRequestHandler<UpdateBlogPostCommand, bool>
{
    private const string CACHE_KEY_ALL_POSTS = "blog_posts_all";
    private const string CACHE_KEY_FEATURED_POSTS = "blog_posts_featured";
    private const string CACHE_KEY_RECENT_POSTS = "blog_posts_recent";

    public async Task<bool> Handle(UpdateBlogPostCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating blog post. PostId: {PostId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var post = await postRepository.GetByIdAsync(request.Id, cancellationToken);
            if (post == null)
            {
                logger.LogWarning("Blog post not found. PostId: {PostId}", request.Id);
                return false;
            }

            if (request.PerformedBy.HasValue && post.AuthorId != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to update blog post {PostId} by user {UserId}. Post belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, post.AuthorId);
                throw new BusinessException("Bu blog post'unu güncelleme yetkiniz bulunmamaktadır.");
            }

            if (request.CategoryId.HasValue && request.CategoryId.Value != Guid.Empty)
            {
                // Category validation
                var category = await context.Set<BlogCategory>()
                    .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value && c.IsActive, cancellationToken);
                if (category == null)
                {
                    throw new NotFoundException("Kategori", request.CategoryId.Value);
                }
                post.UpdateCategory(request.CategoryId.Value);
            }
            if (!string.IsNullOrEmpty(request.Title))
            {
                post.UpdateTitle(request.Title);
            }
            if (!string.IsNullOrEmpty(request.Excerpt))
                post.UpdateExcerpt(request.Excerpt);
            if (!string.IsNullOrEmpty(request.Content))
            {
                post.UpdateContent(request.Content);
                post.UpdateReadingTime(CalculateReadingTime(request.Content));
            }
            if (request.FeaturedImageUrl != null)
                post.UpdateFeaturedImage(request.FeaturedImageUrl);
            if (!string.IsNullOrEmpty(request.Status))
            {
                if (Enum.TryParse<ContentStatus>(request.Status, true, out var newStatus))
                {
                    post.UpdateStatus(newStatus);
                }
            }
            if (request.Tags != null)
            {
                var tags = string.Join(",", request.Tags);
                post.UpdateTags(tags);
            }
            if (request.IsFeatured.HasValue)
            {
                if (request.IsFeatured.Value)
                    post.SetAsFeatured();
                else
                    post.UnsetAsFeatured();
            }
            if (request.AllowComments.HasValue)
                post.UpdateAllowComments(request.AllowComments.Value);
            if (request.MetaTitle != null || request.MetaDescription != null || request.MetaKeywords != null || request.OgImageUrl != null)
                post.UpdateMetaInformation(request.MetaTitle, request.MetaDescription, request.MetaKeywords, request.OgImageUrl);

            await postRepository.UpdateAsync(post, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_POSTS, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_FEATURED_POSTS, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_RECENT_POSTS, cancellationToken);
            await cache.RemoveAsync($"blog_post_{request.Id}", cancellationToken); // Single post cache
            await cache.RemoveAsync($"blog_post_slug_{post.Slug}", cancellationToken); // Slug cache
            if (request.CategoryId.HasValue)
            {
                await cache.RemoveAsync($"blog_posts_category_{request.CategoryId.Value}", cancellationToken); // Category posts cache
            }
            await cache.RemoveAsync($"blog_posts_category_{post.CategoryId}", cancellationToken); // Old category posts cache

            logger.LogInformation("Blog post updated. PostId: {PostId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating blog post Id: {PostId}", request.Id);
            throw new BusinessException("Blog post güncelleme çakışması. Başka bir kullanıcı aynı post'u güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating blog post Id: {PostId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static int CalculateReadingTime(string content)
    {
        // Average reading speed: 200 words per minute
        const int wordsPerMinute = 200;

        // Remove HTML tags for accurate word count
        var textContent = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty);
        var wordCount = textContent.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

        var readingTime = (int)Math.Ceiling((double)wordCount / wordsPerMinute);
        return Math.Max(1, readingTime); // Minimum 1 minute
    }
}

