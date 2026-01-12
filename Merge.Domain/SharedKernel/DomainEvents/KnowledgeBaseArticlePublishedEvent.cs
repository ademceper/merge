using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Knowledge Base Article Published Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record KnowledgeBaseArticlePublishedEvent(
    Guid ArticleId,
    string Title,
    string Slug,
    DateTime PublishedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
