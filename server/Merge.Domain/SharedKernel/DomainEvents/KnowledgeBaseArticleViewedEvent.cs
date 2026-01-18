using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record KnowledgeBaseArticleViewedEvent(
    Guid ArticleId,
    string Title,
    int ViewCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
