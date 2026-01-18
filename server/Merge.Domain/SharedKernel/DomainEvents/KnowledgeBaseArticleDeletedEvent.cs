using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record KnowledgeBaseArticleDeletedEvent(
    Guid ArticleId,
    string Title,
    Guid? CategoryId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
