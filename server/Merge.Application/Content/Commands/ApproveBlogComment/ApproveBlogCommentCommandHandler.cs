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
using IRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.BlogComment>;

namespace Merge.Application.Content.Commands.ApproveBlogComment;

public class ApproveBlogCommentCommandHandler(
    IRepository commentRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<ApproveBlogCommentCommandHandler> logger) : IRequestHandler<ApproveBlogCommentCommand, bool>
{
    private const string CACHE_KEY_POST_COMMENTS = "blog_post_comments_";

    public async Task<bool> Handle(ApproveBlogCommentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Approving blog comment. CommentId: {CommentId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comment = await commentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (comment is null)
            {
                logger.LogWarning("Blog comment not found. CommentId: {CommentId}", request.Id);
                return false;
            }

            comment.Approve();

            await commentRepository.UpdateAsync(comment, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"{CACHE_KEY_POST_COMMENTS}{comment.BlogPostId}", cancellationToken);

            logger.LogInformation("Blog comment approved. CommentId: {CommentId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while approving blog comment Id: {CommentId}", request.Id);
            throw new BusinessException("Blog yorumu onaylama çakışması. Başka bir kullanıcı aynı yorumu güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving blog comment Id: {CommentId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

