using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record KnowledgeBaseArticlePublishedEvent(
    Guid ArticleId,
    string Title,
    string Slug,
    DateTime PublishedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
