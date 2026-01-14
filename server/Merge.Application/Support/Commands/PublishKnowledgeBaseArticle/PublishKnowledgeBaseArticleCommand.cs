using MediatR;

namespace Merge.Application.Support.Commands.PublishKnowledgeBaseArticle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record PublishKnowledgeBaseArticleCommand(
    Guid ArticleId
) : IRequest<bool>;
