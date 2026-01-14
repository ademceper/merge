using MediatR;

namespace Merge.Application.Support.Commands.DeleteKnowledgeBaseArticle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteKnowledgeBaseArticleCommand(
    Guid ArticleId
) : IRequest<bool>;
