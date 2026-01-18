using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseTotalViews;

public class GetKnowledgeBaseTotalViewsQueryHandler(IDbContext context) : IRequestHandler<GetKnowledgeBaseTotalViewsQuery, int>
{

    public async Task<int> Handle(GetKnowledgeBaseTotalViewsQuery request, CancellationToken cancellationToken)
    {
        if (request.ArticleId.HasValue)
        {
            var article = await context.Set<KnowledgeBaseArticle>()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.ArticleId.Value, cancellationToken);

            return article?.ViewCount ?? 0;
        }

        // Total views for all articles
        var totalViews = await context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .SumAsync(a => a.ViewCount, cancellationToken);

        return totalViews;
    }
}
