using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetBlogPosts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetBlogPostsQueryHandler : IRequestHandler<GetBlogPostsQuery, PagedResult<BlogPostDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBlogPostsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_BLOG_POSTS = "blog_posts_paged_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5); // Blog posts change frequently

    public GetBlogPostsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetBlogPostsQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<BlogPostDto>> Handle(GetBlogPostsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving blog posts. CategoryId: {CategoryId}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.CategoryId, request.Status, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_BLOG_POSTS}{request.CategoryId?.ToString() ?? "all"}_{request.Status ?? "all"}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for paginated blog post queries
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for blog posts. CategoryId: {CategoryId}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
                    request.CategoryId, request.Status, page, pageSize);

                // ✅ PERFORMANCE: AsNoTracking for read-only queries
                IQueryable<BlogPost> query = _context.Set<BlogPost>()
                    .AsNoTracking()
                    .Include(p => p.Category)
                    .Include(p => p.Author);

                if (request.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == request.CategoryId.Value);
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    if (Enum.TryParse<ContentStatus>(request.Status, true, out var statusEnum))
                    {
                        query = query.Where(p => p.Status == statusEnum);
                    }
                }

                var orderedQuery = query.OrderByDescending(p => p.PublishedAt ?? p.CreatedAt);
                var totalCount = await orderedQuery.CountAsync(cancellationToken);

                var posts = await orderedQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} blog posts (page {Page})", posts.Count, page);

                return new PagedResult<BlogPostDto>
                {
                    Items = _mapper.Map<List<BlogPostDto>>(posts),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            },
            CACHE_EXPIRATION,
            cancellationToken);

        return cachedResult!;
    }
}

