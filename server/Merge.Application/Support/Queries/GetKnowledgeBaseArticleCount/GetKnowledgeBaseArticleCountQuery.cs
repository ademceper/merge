using MediatR;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseArticleCount;

public record GetKnowledgeBaseArticleCountQuery(
    Guid? CategoryId = null
) : IRequest<int>;
