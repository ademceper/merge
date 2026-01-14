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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class SearchBlogPostsQueryHandler : IRequestHandler<SearchBlogPostsQuery, PagedResult<BlogPostDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchBlogPostsQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public SearchBlogPostsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<SearchBlogPostsQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<BlogPostDto>> Handle(SearchBlogPostsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching blog posts. Query: {Query}, Page: {Page}, PageSize: {PageSize}",
            request.Query, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var query = _context.Set<BlogPost>()
            .AsNoTracking()
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

        _logger.LogInformation("Found {Count} blog posts matching query '{Query}' (page {Page})", posts.Count, request.Query, page);

        return new PagedResult<BlogPostDto>
        {
            Items = _mapper.Map<List<BlogPostDto>>(posts),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

