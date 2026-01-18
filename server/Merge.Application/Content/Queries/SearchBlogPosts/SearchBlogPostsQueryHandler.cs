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

namespace Merge.Application.Content.Queries.SearchBlogPosts;

public class SearchBlogPostsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<SearchBlogPostsQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<SearchBlogPostsQuery, PagedResult<BlogPostDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<BlogPostDto>> Handle(SearchBlogPostsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Searching blog posts. Query: {Query}, Page: {Page}, PageSize: {PageSize}",
            request.Query, request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Set<BlogPost>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.Author)
            .Where(p => p.Status == ContentStatus.Published &&
                       (p.Title.Contains(request.Query) || p.Content.Contains(request.Query) || p.Excerpt.Contains(request.Query)));

        var totalCount = await query.CountAsync(cancellationToken);

        var posts = await query
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Found {Count} blog posts matching query '{Query}' (page {Page})", posts.Count, request.Query, page);

        return new PagedResult<BlogPostDto>
        {
            Items = mapper.Map<List<BlogPostDto>>(posts),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

