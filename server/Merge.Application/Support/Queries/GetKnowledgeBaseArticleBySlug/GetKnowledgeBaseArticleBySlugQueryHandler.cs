using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Content;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseArticleBySlug;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetKnowledgeBaseArticleBySlugQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetKnowledgeBaseArticleBySlugQuery, KnowledgeBaseArticleDto?>
{

    public async Task<KnowledgeBaseArticleDto?> Handle(GetKnowledgeBaseArticleBySlugQuery request, CancellationToken cancellationToken)
    {
        var article = await context.Set<KnowledgeBaseArticle>()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Slug == request.Slug && a.Status == ContentStatus.Published, cancellationToken);

        return article != null ? mapper.Map<KnowledgeBaseArticleDto>(article) : null;
    }
}
