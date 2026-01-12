using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseTotalViews;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetKnowledgeBaseTotalViewsQueryHandler : IRequestHandler<GetKnowledgeBaseTotalViewsQuery, int>
{
    private readonly IDbContext _context;

    public GetKnowledgeBaseTotalViewsQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(GetKnowledgeBaseTotalViewsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Database'de sum yap, memory'de işlem YASAK
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (request.ArticleId.HasValue)
        {
            var article = await _context.Set<KnowledgeBaseArticle>()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.ArticleId.Value, cancellationToken);

            return article?.ViewCount ?? 0;
        }

        // Total views for all articles
        var totalViews = await _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .SumAsync(a => a.ViewCount, cancellationToken);

        return totalViews;
    }
}
