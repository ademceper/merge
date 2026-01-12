using MediatR;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseTotalViews;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetKnowledgeBaseTotalViewsQuery(
    Guid? ArticleId = null
) : IRequest<int>;
