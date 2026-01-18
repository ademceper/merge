using MediatR;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseTotalViews;

public record GetKnowledgeBaseTotalViewsQuery(
    Guid? ArticleId = null
) : IRequest<int>;
