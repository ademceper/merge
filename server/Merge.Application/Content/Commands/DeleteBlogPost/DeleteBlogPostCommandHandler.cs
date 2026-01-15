using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Commands.DeleteBlogPost;

public class DeleteBlogPostCommandHandler(
    Merge.Application.Interfaces.IRepository<BlogPost> postRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<DeleteBlogPostCommandHandler> logger) : IRequestHandler<DeleteBlogPostCommand, bool>
{
    private const string CACHE_KEY_ALL_POSTS = "blog_posts_all";
    private const string CACHE_KEY_FEATURED_POSTS = "blog_posts_featured";
    private const string CACHE_KEY_RECENT_POSTS = "blog_posts_recent";

    public async Task<bool> Handle(DeleteBlogPostCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting blog post. PostId: {PostId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var post = await postRepository.GetByIdAsync(request.Id, cancellationToken);
            if (post == null)
            {
                logger.LogWarning("Blog post not found for deletion. PostId: {PostId}", request.Id);
                return false;
            }

            if (request.PerformedBy.HasValue && post.AuthorId != request.PerformedBy.Value)
            {
                logger.LogWarning("Unauthorized attempt to delete blog post {PostId} by user {UserId}. Post belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, post.AuthorId);
                throw new BusinessException("Bu blog post'unu silme yetkiniz bulunmamaktadır.");
            }

            post.MarkAsDeleted();

            await postRepository.UpdateAsync(post, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_POSTS, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_FEATURED_POSTS, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_RECENT_POSTS, cancellationToken);
            await cache.RemoveAsync($"blog_post_{request.Id}", cancellationToken);
            await cache.RemoveAsync($"blog_post_slug_{post.Slug}", cancellationToken);
            await cache.RemoveAsync($"blog_posts_category_{post.CategoryId}", cancellationToken);

            logger.LogInformation("Blog post deleted (soft delete). PostId: {PostId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while deleting blog post Id: {PostId}", request.Id);
            throw new BusinessException("Blog post silme çakışması. Başka bir kullanıcı aynı post'u güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting blog post Id: {PostId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

