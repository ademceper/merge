using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.SearchKnowledgeBaseArticles;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class SearchKnowledgeBaseArticlesQueryHandler(IDbContext context, IMapper mapper, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<SearchKnowledgeBaseArticlesQuery, PagedResult<KnowledgeBaseArticleDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<KnowledgeBaseArticleDto>> Handle(SearchKnowledgeBaseArticlesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        var pageSize = request.PageSize > 0 && request.PageSize <= paginationConfig.MaxPageSize 
            ? request.PageSize 
            : paginationConfig.DefaultPageSize;
        var page = request.Page > 0 ? request.Page : 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        IQueryable<KnowledgeBaseArticle> query = context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Where(a => a.Status == ContentStatus.Published);

        if (!string.IsNullOrEmpty(request.Query))
        {
            query = query.Where(a => 
                a.Title.Contains(request.Query) ||
                a.Content.Contains(request.Query) ||
                (a.Excerpt != null && a.Excerpt.Contains(request.Query)) ||
                (a.Tags != null && a.Tags.Contains(request.Query)));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == request.CategoryId.Value);
        }

        if (request.FeaturedOnly)
        {
            query = query.Where(a => a.IsFeatured);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var articles = await query
            .OrderByDescending(a => a.IsFeatured)
            .ThenByDescending(a => a.ViewCount)
            .ThenByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return new PagedResult<KnowledgeBaseArticleDto>
        {
            Items = mapper.Map<List<KnowledgeBaseArticleDto>>(articles),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
