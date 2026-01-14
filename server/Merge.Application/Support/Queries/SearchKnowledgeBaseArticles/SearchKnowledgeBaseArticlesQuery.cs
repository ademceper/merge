using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;

namespace Merge.Application.Support.Queries.SearchKnowledgeBaseArticles;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SearchKnowledgeBaseArticlesQuery(
    string? Query = null,
    Guid? CategoryId = null,
    bool FeaturedOnly = false,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<KnowledgeBaseArticleDto>>;
