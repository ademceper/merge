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

namespace Merge.Application.Content.Queries.GetBlogPostComments;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetBlogPostCommentsQueryHandler : IRequestHandler<GetBlogPostCommentsQuery, PagedResult<BlogCommentDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBlogPostCommentsQueryHandler> _logger;
    private readonly ICacheService _cache;
    private readonly PaginationSettings _paginationSettings;
    private const string CACHE_KEY_POST_COMMENTS = "blog_post_comments_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5); // Comments change frequently

    public GetBlogPostCommentsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetBlogPostCommentsQueryHandler> logger,
        ICacheService cache,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<BlogCommentDto>> Handle(GetBlogPostCommentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving blog post comments. PostId: {PostId}, IsApproved: {IsApproved}, Page: {Page}, PageSize: {PageSize}",
            request.PostId, request.IsApproved, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_POST_COMMENTS}{request.PostId}_{request.IsApproved?.ToString() ?? "all"}_{page}_{pageSize}";

        // ✅ BOLUM 10.2: Redis distributed cache for paginated blog post comments
        var cachedResult = await _cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                _logger.LogInformation("Cache miss for blog post comments. PostId: {PostId}, IsApproved: {IsApproved}, Page: {Page}, PageSize: {PageSize}",
                    request.PostId, request.IsApproved, page, pageSize);

                // ✅ PERFORMANCE: AsNoTracking for read-only queries
                IQueryable<BlogComment> query = _context.Set<BlogComment>()
                    .AsNoTracking()
                    .Include(c => c.User)
                    .Include(c => c.Replies)
                    .Where(c => c.BlogPostId == request.PostId && c.ParentCommentId == null);

                if (request.IsApproved.HasValue)
                {
                    query = query.Where(c => c.IsApproved == request.IsApproved.Value);
                }

                var totalCount = await query.CountAsync(cancellationToken);

                var comments = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Retrieved {Count} blog post comments (page {Page})", comments.Count, page);

                // ✅ PERFORMANCE: Recursive mapping için Replies'leri manuel set et (AutoMapper recursive mapping'i desteklemiyor)
                var commentDtos = new List<BlogCommentDto>();
                foreach (var comment in comments)
                {
                    var commentDto = _mapper.Map<BlogCommentDto>(comment);
                    if (comment.Replies != null && comment.Replies.Any())
                    {
                        var replies = comment.Replies
                            .Where(r => !request.IsApproved.HasValue || r.IsApproved == request.IsApproved.Value)
                            .OrderBy(r => r.CreatedAt)
                            .Select(r => _mapper.Map<BlogCommentDto>(r))
                            .ToList();
                        // ✅ BOLUM 7.1: Records - with expression ile computed property güncelleme
                        commentDto = commentDto with 
                        { 
                            Replies = replies.AsReadOnly(), 
                            ReplyCount = replies.Count 
                        };
                    }
                    commentDtos.Add(commentDto);
                }

                return new PagedResult<BlogCommentDto>
                {
                    Items = commentDtos,
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

