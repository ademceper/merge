using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record KnowledgeBaseArticleMarkedAsHelpfulEvent(
    Guid ArticleId,
    string Title,
    int HelpfulCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
