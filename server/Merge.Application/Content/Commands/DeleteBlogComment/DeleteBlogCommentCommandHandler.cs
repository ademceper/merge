using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using ICommentRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.BlogComment>;
using IPostRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.BlogPost>;

namespace Merge.Application.Content.Commands.DeleteBlogComment;

public class DeleteBlogCommentCommandHandler(
    ICommentRepository commentRepository,
    IPostRepository postRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<DeleteBlogCommentCommandHandler> logger) : IRequestHandler<DeleteBlogCommentCommand, bool>
{
    private const string CACHE_KEY_POST_COMMENTS = "blog_post_comments_";

    public async Task<bool> Handle(DeleteBlogCommentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting blog comment. CommentId: {CommentId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comment = await commentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (comment == null)
            {
                logger.LogWarning("Blog comment not found for deletion. CommentId: {CommentId}", request.Id);
                return false;
            }

            var blogPostId = comment.BlogPostId;

            comment.MarkAsDeleted();

            await commentRepository.UpdateAsync(comment, cancellationToken);

            var post = await postRepository.GetByIdAsync(blogPostId, cancellationToken);
            if (post != null)
            {
                post.DecrementCommentCount();
                await postRepository.UpdateAsync(post, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_POST_COMMENTS}{blogPostId}", cancellationToken);

            logger.LogInformation("Blog comment deleted (soft delete). CommentId: {CommentId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while deleting blog comment Id: {CommentId}", request.Id);
            throw new BusinessException("Blog yorumu silme çakışması. Başka bir kullanıcı aynı yorumu güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting blog comment Id: {CommentId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

