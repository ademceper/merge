using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Content.Commands.CreateBlogPost;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateBlogPostCommandHandler : IRequestHandler<CreateBlogPostCommand, BlogPostDto>
{
    private readonly IRepository<BlogPost> _postRepository;
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateBlogPostCommandHandler> _logger;
    private const string CACHE_KEY_ALL_POSTS = "blog_posts_all";
    private const string CACHE_KEY_FEATURED_POSTS = "blog_posts_featured";
    private const string CACHE_KEY_RECENT_POSTS = "blog_posts_recent";

    public CreateBlogPostCommandHandler(
        IRepository<BlogPost> postRepository,
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreateBlogPostCommandHandler> logger)
    {
        _postRepository = postRepository;
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BlogPostDto> Handle(CreateBlogPostCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating blog post. AuthorId: {AuthorId}, CategoryId: {CategoryId}, Title: {Title}",
            request.AuthorId, request.CategoryId, request.Title);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Category validation
            var category = await _context.Set<BlogCategory>()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive, cancellationToken);

            if (category == null)
            {
                _logger.LogWarning("Blog post creation failed: Category not found. CategoryId: {CategoryId}", request.CategoryId);
                throw new NotFoundException("Kategori", request.CategoryId);
            }

            // Slug uniqueness check
            var slug = GenerateSlug(request.Title);
            if (await _context.Set<BlogPost>().AnyAsync(p => p.Slug == slug, cancellationToken))
            {
                slug = $"{slug}-{DateTime.UtcNow.Ticks}";
            }

            // Calculate reading time
            var readingTime = CalculateReadingTime(request.Content);

            // Parse status enum
            if (!Enum.TryParse<ContentStatus>(request.Status, true, out var statusEnum))
            {
                statusEnum = ContentStatus.Draft;
            }

            // Convert tags list to comma-separated string
            var tags = request.Tags != null ? string.Join(",", request.Tags) : null;

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var post = BlogPost.Create(
                request.CategoryId,
                request.AuthorId,
                request.Title,
                request.Excerpt,
                request.Content,
                request.FeaturedImageUrl,
                statusEnum,
                tags,
                request.IsFeatured,
                request.AllowComments,
                request.MetaTitle,
                request.MetaDescription,
                request.MetaKeywords,
                request.OgImageUrl,
                readingTime,
                slug);

            post = await _postRepository.AddAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            var reloadedPost = await _context.Set<BlogPost>()
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == post.Id, cancellationToken);

            if (reloadedPost == null)
            {
                _logger.LogWarning("Blog post {PostId} not found after creation", post.Id);
                throw new NotFoundException("Blog Post", post.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_POSTS, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_FEATURED_POSTS, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_RECENT_POSTS, cancellationToken);
            await _cache.RemoveAsync($"blog_post_{post.Id}", cancellationToken); // Single post cache
            await _cache.RemoveAsync($"blog_post_slug_{post.Slug}", cancellationToken); // Slug cache
            await _cache.RemoveAsync($"blog_posts_category_{request.CategoryId}", cancellationToken); // Category posts cache

            _logger.LogInformation("Blog post created. PostId: {PostId}, Slug: {Slug}, AuthorId: {AuthorId}",
                post.Id, post.Slug, request.AuthorId);

            return _mapper.Map<BlogPostDto>(reloadedPost);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while creating blog post with Title: {Title}", request.Title);
            throw new BusinessException("Blog post oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error creating blog post with Title: {Title}", request.Title);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ç", "c")
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(":", "")
            .Replace(";", "");

        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return slug.Trim('-');
    }

    private static int CalculateReadingTime(string content)
    {
        // Average reading speed: 200 words per minute
        // Average word length: 5 characters
        const int wordsPerMinute = 200;
        const int averageWordLength = 5;

        // Remove HTML tags for accurate word count
        var textContent = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty);
        var wordCount = textContent.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;

        var readingTime = (int)Math.Ceiling((double)wordCount / wordsPerMinute);
        return Math.Max(1, readingTime); // Minimum 1 minute
    }
}

