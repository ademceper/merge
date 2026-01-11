using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Knowledge Base Article Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record KnowledgeBaseArticleCreatedEvent(
    Guid ArticleId,
    string Title,
    string Slug,
    Guid? AuthorId,
    Guid? CategoryId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
