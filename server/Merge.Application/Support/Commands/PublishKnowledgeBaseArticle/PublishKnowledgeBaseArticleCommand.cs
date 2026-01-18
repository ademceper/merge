using MediatR;

namespace Merge.Application.Support.Commands.PublishKnowledgeBaseArticle;

public record PublishKnowledgeBaseArticleCommand(
    Guid ArticleId
) : IRequest<bool>;
