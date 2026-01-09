using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Content.Queries.GetBlogPostBySlug;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetBlogPostBySlugQueryHandler : IRequestHandler<GetBlogPostBySlugQuery, BlogPostDto?>
{
    private readonly IDbContext _context;
    private readonly IRepository<BlogPost> _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBlogPostBySlugQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_POST_BY_SLUG = "blog_post_slug_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5); // Blog posts change frequently

    public GetBlogPostBySlugQueryHandler(
        IDbContext context,
        IRepository<BlogPost> postRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetBlogPostBySlugQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<BlogPostDto?> Handle(GetBlogPostBySlugQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving blog post with Slug: {Slug}, TrackView: {TrackView}", request.Slug, request.TrackView);

        var cacheKey = $"{CACHE_KEY_POST_BY_SLUG}{request.Slug}";

        // ✅ BOLUM 10.2: Redis distributed cache for single blog post by slug
        var cachedPost = await _cache.GetAsync<BlogPostDto>(cacheKey, cancellationToken);
        if (cachedPost != null && !request.TrackView) // Don't use cache if tracking view (need to update)
        {
            _logger.LogInformation("Cache hit for blog post. Slug: {Slug}", request.Slug);
            return cachedPost;
        }

        _logger.LogInformation("Cache miss for blog post. Slug: {Slug}", request.Slug);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries (unless tracking view)
        var post = request.TrackView
            ? await _context.Set<BlogPost>()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Slug == request.Slug && p.Status == ContentStatus.Published, cancellationToken)
            : await _context.Set<BlogPost>()
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Slug == request.Slug && p.Status == ContentStatus.Published, cancellationToken);

        if (post == null)
        {
            _logger.LogWarning("Blog post not found with Slug: {Slug}", request.Slug);
            return null;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.TrackView)
        {
            post.IncrementViewCount();
            await _postRepository.UpdateAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Successfully retrieved blog post {PostId} by slug {Slug}", post.Id, request.Slug);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var postDto = _mapper.Map<BlogPostDto>(post);

        // Cache the result (only if not tracking view)
        if (!request.TrackView)
        {
            await _cache.SetAsync(cacheKey, postDto, CACHE_EXPIRATION, cancellationToken);
        }

        return postDto;
    }
}

