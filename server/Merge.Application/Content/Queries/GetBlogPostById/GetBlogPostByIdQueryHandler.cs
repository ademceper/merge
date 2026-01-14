using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetBlogPostById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetBlogPostByIdQueryHandler : IRequestHandler<GetBlogPostByIdQuery, BlogPostDto?>
{
    private readonly IDbContext _context;
    private readonly Merge.Application.Interfaces.IRepository<BlogPost> _postRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBlogPostByIdQueryHandler> _logger;
    private readonly ICacheService _cache;
    private const string CACHE_KEY_POST_BY_ID = "blog_post_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5); // Blog posts change frequently

    public GetBlogPostByIdQueryHandler(
        IDbContext context,
        Merge.Application.Interfaces.IRepository<BlogPost> postRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetBlogPostByIdQueryHandler> logger,
        ICacheService cache)
    {
        _context = context;
        _postRepository = postRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<BlogPostDto?> Handle(GetBlogPostByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving blog post with Id: {PostId}, TrackView: {TrackView}", request.Id, request.TrackView);

        var cacheKey = $"{CACHE_KEY_POST_BY_ID}{request.Id}";

        // ✅ BOLUM 10.2: Redis distributed cache for single blog post
        var cachedPost = await _cache.GetAsync<BlogPostDto>(cacheKey, cancellationToken);
        if (cachedPost != null && !request.TrackView) // Don't use cache if tracking view (need to update)
        {
            _logger.LogInformation("Cache hit for blog post. PostId: {PostId}", request.Id);
            return cachedPost;
        }

        _logger.LogInformation("Cache miss for blog post. PostId: {PostId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries (unless tracking view)
        var post = request.TrackView
            ? await _context.Set<BlogPost>()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            : await _context.Set<BlogPost>()
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (post == null)
        {
            _logger.LogWarning("Blog post not found with Id: {PostId}", request.Id);
            return null;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.TrackView && post.Status == ContentStatus.Published)
        {
            post.IncrementViewCount();
            await _postRepository.UpdateAsync(post, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Successfully retrieved blog post {PostId}", request.Id);

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

