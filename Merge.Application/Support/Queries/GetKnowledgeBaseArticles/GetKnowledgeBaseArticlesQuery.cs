using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseArticles;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetKnowledgeBaseArticlesQuery(
    string? Status = null,
    Guid? CategoryId = null,
    bool FeaturedOnly = false,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<KnowledgeBaseArticleDto>>;
