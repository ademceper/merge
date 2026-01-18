using MediatR;

namespace Merge.Application.Support.Commands.RecordKnowledgeBaseArticleView;

public record RecordKnowledgeBaseArticleViewCommand(
    Guid ArticleId,
    Guid? UserId = null,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<bool>;
