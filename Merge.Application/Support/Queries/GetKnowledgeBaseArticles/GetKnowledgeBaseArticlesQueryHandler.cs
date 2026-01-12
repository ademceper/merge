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

namespace Merge.Application.Support.Queries.GetKnowledgeBaseArticles;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetKnowledgeBaseArticlesQueryHandler : IRequestHandler<GetKnowledgeBaseArticlesQuery, PagedResult<KnowledgeBaseArticleDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly SupportSettings _settings;

    public GetKnowledgeBaseArticlesQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _mapper = mapper;
        _settings = settings.Value;
    }

    public async Task<PagedResult<KnowledgeBaseArticleDto>> Handle(GetKnowledgeBaseArticlesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        var pageSize = request.PageSize > 0 && request.PageSize <= _settings.MaxPageSize 
            ? request.PageSize 
            : _settings.DefaultPageSize;
        var page = request.Page > 0 ? request.Page : 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        IQueryable<KnowledgeBaseArticle> query = _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Category)
            .Include(a => a.Author);

        if (!string.IsNullOrEmpty(request.Status))
        {
            var statusEnum = Enum.Parse<ContentStatus>(request.Status, true);
            query = query.Where(a => a.Status == statusEnum);
        }
        else
        {
            query = query.Where(a => a.Status == ContentStatus.Published);
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
            .OrderBy(a => a.DisplayOrder)
            .ThenByDescending(a => a.PublishedAt ?? a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return new PagedResult<KnowledgeBaseArticleDto>
        {
            Items = _mapper.Map<List<KnowledgeBaseArticleDto>>(articles),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
