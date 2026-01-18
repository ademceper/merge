using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseArticleCount;

public class GetKnowledgeBaseArticleCountQueryHandler(IDbContext context) : IRequestHandler<GetKnowledgeBaseArticleCountQuery, int>
{

    public async Task<int> Handle(GetKnowledgeBaseArticleCountQuery request, CancellationToken cancellationToken)
    {
        IQueryable<KnowledgeBaseArticle> query = context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Where(a => a.Status == ContentStatus.Published);

        if (request.CategoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == request.CategoryId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }
}
