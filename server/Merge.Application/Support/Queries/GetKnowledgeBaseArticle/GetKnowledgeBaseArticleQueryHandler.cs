using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseArticle;

public class GetKnowledgeBaseArticleQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetKnowledgeBaseArticleQuery, KnowledgeBaseArticleDto?>
{

    public async Task<KnowledgeBaseArticleDto?> Handle(GetKnowledgeBaseArticleQuery request, CancellationToken cancellationToken)
    {
        var article = await context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == request.ArticleId, cancellationToken);

        return article is not null ? mapper.Map<KnowledgeBaseArticleDto>(article) : null;
    }
}
