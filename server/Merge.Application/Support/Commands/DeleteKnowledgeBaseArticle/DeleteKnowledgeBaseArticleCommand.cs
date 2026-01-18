using MediatR;

namespace Merge.Application.Support.Commands.DeleteKnowledgeBaseArticle;

public record DeleteKnowledgeBaseArticleCommand(
    Guid ArticleId
) : IRequest<bool>;
