using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Commands.ApproveBlogComment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ApproveBlogCommentCommandHandler : IRequestHandler<ApproveBlogCommentCommand, bool>
{
    private readonly IRepository<BlogComment> _commentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<ApproveBlogCommentCommandHandler> _logger;
    private const string CACHE_KEY_POST_COMMENTS = "blog_post_comments_";

    public ApproveBlogCommentCommandHandler(
        IRepository<BlogComment> commentRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<ApproveBlogCommentCommandHandler> logger)
    {
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(ApproveBlogCommentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving blog comment. CommentId: {CommentId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var comment = await _commentRepository.GetByIdAsync(request.Id, cancellationToken);
            if (comment == null)
            {
                _logger.LogWarning("Blog comment not found. CommentId: {CommentId}", request.Id);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            comment.Approve();

            await _commentRepository.UpdateAsync(comment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"{CACHE_KEY_POST_COMMENTS}{comment.BlogPostId}", cancellationToken);

            _logger.LogInformation("Blog comment approved. CommentId: {CommentId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while approving blog comment Id: {CommentId}", request.Id);
            throw new BusinessException("Blog yorumu onaylama çakışması. Başka bir kullanıcı aynı yorumu güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error approving blog comment Id: {CommentId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

