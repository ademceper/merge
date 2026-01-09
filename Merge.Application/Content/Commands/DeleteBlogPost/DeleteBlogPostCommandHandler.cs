using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Content.Commands.DeleteBlogPost;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteBlogPostCommandHandler : IRequestHandler<DeleteBlogPostCommand, bool>
{
    private readonly IRepository<BlogPost> _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteBlogPostCommandHandler> _logger;
    private const string CACHE_KEY_ALL_POSTS = "blog_posts_all";
    private const string CACHE_KEY_FEATURED_POSTS = "blog_posts_featured";
    private const string CACHE_KEY_RECENT_POSTS = "blog_posts_recent";

    public DeleteBlogPostCommandHandler(
        IRepository<BlogPost> postRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeleteBlogPostCommandHandler> logger)
    {
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteBlogPostCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting blog post. PostId: {PostId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken);
            if (post == null)
            {
                _logger.LogWarning("Blog post not found for deletion. PostId: {PostId}", request.Id);
                return false;
            }

            // ✅ BOLUM 3.2: IDOR Korumasi - Manager sadece kendi post'larını silebilmeli (Admin hariç)
            if (request.PerformedBy.HasValue && post.AuthorId != request.PerformedBy.Value)
            {
                _logger.LogWarning("Unauthorized attempt to delete blog post {PostId} by user {UserId}. Post belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, post.AuthorId);
                throw new BusinessException("Bu blog post'unu silme yetkiniz bulunmamaktadır.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (soft delete)
            post.MarkAsDeleted();

            await _postRepository.UpdateAsync(post, cancellationToken); // Soft delete olduğu için Update
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_POSTS, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_FEATURED_POSTS, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_RECENT_POSTS, cancellationToken);
            await _cache.RemoveAsync($"blog_post_{request.Id}", cancellationToken); // Single post cache
            await _cache.RemoveAsync($"blog_post_slug_{post.Slug}", cancellationToken); // Slug cache
            await _cache.RemoveAsync($"blog_posts_category_{post.CategoryId}", cancellationToken); // Category posts cache

            _logger.LogInformation("Blog post deleted (soft delete). PostId: {PostId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while deleting blog post Id: {PostId}", request.Id);
            throw new BusinessException("Blog post silme çakışması. Başka bir kullanıcı aynı post'u güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error deleting blog post Id: {PostId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

