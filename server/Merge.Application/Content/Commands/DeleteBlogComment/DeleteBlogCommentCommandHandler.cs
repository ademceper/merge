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

namespace Merge.Application.Content.Commands.DeleteBlogComment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteBlogCommentCommandHandler : IRequestHandler<DeleteBlogCommentCommand, bool>
{
    private readonly Merge.Application.Interfaces.IRepository<BlogComment> _commentRepository;
    private readonly Merge.Application.Interfaces.IRepository<BlogPost> _postRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteBlogCommentCommandHandler> _logger;
    private const string CACHE_KEY_POST_COMMENTS = "blog_post_comments_";

    public DeleteBlogCommentCommandHandler(
        Merge.Application.Interfaces.IRepository<BlogComment> commentRepository,
        Merge.Application.Interfaces.IRepository<BlogPost> postRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeleteBlogCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteBlogCommentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting blog comment. CommentId: {CommentId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comment = await _commentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (comment == null)
            {
                _logger.LogWarning("Blog comment not found for deletion. CommentId: {CommentId}", request.Id);
                return false;
            }

            var blogPostId = comment.BlogPostId;

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            comment.MarkAsDeleted();

            await _commentRepository.UpdateAsync(comment, cancellationToken); // Soft delete olduğu için Update

            // Decrement post comment count
            var post = await _postRepository.GetByIdAsync(blogPostId, cancellationToken);
            if (post != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                post.DecrementCommentCount();
                await _postRepository.UpdateAsync(post, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_POST_COMMENTS}{blogPostId}", cancellationToken);

            _logger.LogInformation("Blog comment deleted (soft delete). CommentId: {CommentId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while deleting blog comment Id: {CommentId}", request.Id);
            throw new BusinessException("Blog yorumu silme çakışması. Başka bir kullanıcı aynı yorumu güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error deleting blog comment Id: {CommentId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

