using MediatR;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseArticleCount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetKnowledgeBaseArticleCountQuery(
    Guid? CategoryId = null
) : IRequest<int>;
