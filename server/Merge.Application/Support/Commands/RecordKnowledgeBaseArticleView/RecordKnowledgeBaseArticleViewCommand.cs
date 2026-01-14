using MediatR;

namespace Merge.Application.Support.Commands.RecordKnowledgeBaseArticleView;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RecordKnowledgeBaseArticleViewCommand(
    Guid ArticleId,
    Guid? UserId = null,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<bool>;
