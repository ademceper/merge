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
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Content.Queries.GetBlogPostComments;

public class GetBlogPostCommentsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetBlogPostCommentsQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetBlogPostCommentsQuery, PagedResult<BlogCommentDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;
    private const string CACHE_KEY_POST_COMMENTS = "blog_post_comments_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<PagedResult<BlogCommentDto>> Handle(GetBlogPostCommentsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving blog post comments. PostId: {PostId}, IsApproved: {IsApproved}, Page: {Page}, PageSize: {PageSize}",
            request.PostId, request.IsApproved, request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_POST_COMMENTS}{request.PostId}_{request.IsApproved?.ToString() ?? "all"}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for blog post comments. PostId: {PostId}, IsApproved: {IsApproved}, Page: {Page}, PageSize: {PageSize}",
                    request.PostId, request.IsApproved, page, pageSize);

                IQueryable<BlogComment> query = context.Set<BlogComment>()
                    .AsNoTracking()
                    .AsSplitQuery()
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

                logger.LogInformation("Retrieved {Count} blog post comments (page {Page})", comments.Count, page);

                List<BlogCommentDto> commentDtos = [];
                foreach (var comment in comments)
                {
                    var commentDto = mapper.Map<BlogCommentDto>(comment);
                    if (comment.Replies != null && comment.Replies.Any())
                    {
                        var replies = comment.Replies
                            .Where(r => !request.IsApproved.HasValue || r.IsApproved == request.IsApproved.Value)
                            .OrderBy(r => r.CreatedAt)
                            .Select(r => mapper.Map<BlogCommentDto>(r))
                            .ToList();
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

