using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using ICommentRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.BlogComment>;
using IPostRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Content.BlogPost>;

namespace Merge.Application.Content.Commands.CreateBlogComment;

public class CreateBlogCommentCommandHandler(
    ICommentRepository commentRepository,
    IPostRepository postRepository,
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreateBlogCommentCommandHandler> logger) : IRequestHandler<CreateBlogCommentCommand, BlogCommentDto>
{
    private const string CACHE_KEY_POST_COMMENTS = "blog_post_comments_";

    public async Task<BlogCommentDto> Handle(CreateBlogCommentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating blog comment. BlogPostId: {BlogPostId}, UserId: {UserId}",
            request.BlogPostId, request.UserId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var post = await context.Set<BlogPost>()
                .FirstOrDefaultAsync(p => p.Id == request.BlogPostId && p.AllowComments, cancellationToken);

            if (post == null)
            {
                logger.LogWarning("Blog post not found or comments not allowed. BlogPostId: {BlogPostId}", request.BlogPostId);
                throw new NotFoundException("Blog yazısı", request.BlogPostId);
            }

            var autoApprove = request.UserId.HasValue;
            var comment = BlogComment.Create(
                request.BlogPostId,
                request.Content,
                request.UserId,
                request.ParentCommentId,
                request.AuthorName,
                request.AuthorEmail,
                autoApprove);

            comment = await commentRepository.AddAsync(comment, cancellationToken);

            post.IncrementCommentCount();

            await postRepository.UpdateAsync(post, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedComment = await context.Set<BlogComment>()
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.ParentComment)
                .FirstOrDefaultAsync(c => c.Id == comment.Id, cancellationToken);

            if (reloadedComment == null)
            {
                logger.LogWarning("Blog comment {CommentId} not found after creation", comment.Id);
                throw new NotFoundException("Blog Yorumu", comment.Id);
            }

            await cache.RemoveAsync($"{CACHE_KEY_POST_COMMENTS}{request.BlogPostId}", cancellationToken);

            logger.LogInformation("Blog comment created. CommentId: {CommentId}, BlogPostId: {BlogPostId}",
                comment.Id, request.BlogPostId);

            return mapper.Map<BlogCommentDto>(reloadedComment);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while creating blog comment for BlogPostId: {BlogPostId}", request.BlogPostId);
            throw new BusinessException("Blog yorumu oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating blog comment for BlogPostId: {BlogPostId}", request.BlogPostId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

