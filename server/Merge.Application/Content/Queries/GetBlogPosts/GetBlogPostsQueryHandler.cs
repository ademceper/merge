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

public class GetBlogPostsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetBlogPostsQueryHandler> logger,
    ICacheService cache,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetBlogPostsQuery, PagedResult<BlogPostDto>>
{
    private const string CACHE_KEY_BLOG_POSTS = "blog_posts_paged_";
    private static readonly TimeSpan CACHE_EXPIRATION = TimeSpan.FromMinutes(5);

    public async Task<PagedResult<BlogPostDto>> Handle(GetBlogPostsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving blog posts. CategoryId: {CategoryId}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.CategoryId, request.Status, request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationSettings.Value.MaxPageSize ? paginationSettings.Value.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var cacheKey = $"{CACHE_KEY_BLOG_POSTS}{request.CategoryId?.ToString() ?? "all"}_{request.Status ?? "all"}_{page}_{pageSize}";

        var cachedResult = await cache.GetOrCreateAsync(
            cacheKey,
            async () =>
            {
                logger.LogInformation("Cache miss for blog posts. CategoryId: {CategoryId}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
                    request.CategoryId, request.Status, page, pageSize);

                IQueryable<BlogPost> query = context.Set<BlogPost>()
                    .AsNoTracking()
                    .AsSplitQuery()
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

                logger.LogInformation("Retrieved {Count} blog posts (page {Page})", posts.Count, page);

                return new PagedResult<BlogPostDto>
                {
                    Items = mapper.Map<List<BlogPostDto>>(posts),
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

