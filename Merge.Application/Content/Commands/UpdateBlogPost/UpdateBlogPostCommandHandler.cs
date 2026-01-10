using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Commands.UpdateBlogPost;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateBlogPostCommandHandler : IRequestHandler<UpdateBlogPostCommand, bool>
{
    private readonly IRepository<BlogPost> _postRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<UpdateBlogPostCommandHandler> _logger;
    private const string CACHE_KEY_ALL_POSTS = "blog_posts_all";
    private const string CACHE_KEY_FEATURED_POSTS = "blog_posts_featured";
    private const string CACHE_KEY_RECENT_POSTS = "blog_posts_recent";

    public UpdateBlogPostCommandHandler(
        IRepository<BlogPost> postRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<UpdateBlogPostCommandHandler> logger)
    {
        _postRepository = postRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateBlogPostCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating blog post. PostId: {PostId}", request.Id);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken);
            if (post == null)
            {
                _logger.LogWarning("Blog post not found. PostId: {PostId}", request.Id);
                return false;
            }

            // ✅ BOLUM 3.2: IDOR Korumasi - Writer sadece kendi post'larını güncelleyebilmeli (Admin/Manager hariç)
            if (request.PerformedBy.HasValue && post.AuthorId != request.PerformedBy.Value)
            {
                _logger.LogWarning("Unauthorized attempt to update blog post {PostId} by user {UserId}. Post belongs to {AuthorId}",
                    request.Id, request.PerformedBy.Value, post.AuthorId);
                throw new BusinessException("Bu blog post'unu güncelleme yetkiniz bulunmamaktadır.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            if (request.CategoryId.HasValue && request.CategoryId.Value != Guid.Empty)
            {
                // Category validation
                var category = await _context.Set<BlogCategory>()
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
                // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
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

            await _postRepository.UpdateAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_POSTS, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_FEATURED_POSTS, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_RECENT_POSTS, cancellationToken);
            await _cache.RemoveAsync($"blog_post_{request.Id}", cancellationToken); // Single post cache
            await _cache.RemoveAsync($"blog_post_slug_{post.Slug}", cancellationToken); // Slug cache
            if (request.CategoryId.HasValue)
            {
                await _cache.RemoveAsync($"blog_posts_category_{request.CategoryId.Value}", cancellationToken); // Category posts cache
            }
            await _cache.RemoveAsync($"blog_posts_category_{post.CategoryId}", cancellationToken); // Old category posts cache

            _logger.LogInformation("Blog post updated. PostId: {PostId}", request.Id);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while updating blog post Id: {PostId}", request.Id);
            throw new BusinessException("Blog post güncelleme çakışması. Başka bir kullanıcı aynı post'u güncelledi. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error updating blog post Id: {PostId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
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

