using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record KnowledgeBaseArticleMarkedAsNotHelpfulEvent(
    Guid ArticleId,
    string Title,
    int NotHelpfulCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
