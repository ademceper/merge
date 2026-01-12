using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseArticleCount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetKnowledgeBaseArticleCountQueryHandler : IRequestHandler<GetKnowledgeBaseArticleCountQuery, int>
{
    private readonly IDbContext _context;

    public GetKnowledgeBaseArticleCountQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(GetKnowledgeBaseArticleCountQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query, Global Query Filter otomatik uygulanır
        IQueryable<KnowledgeBaseArticle> query = _context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Where(a => a.Status == ContentStatus.Published);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == request.CategoryId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}
