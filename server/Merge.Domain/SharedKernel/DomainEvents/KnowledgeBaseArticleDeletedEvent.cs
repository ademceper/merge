using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Knowledge Base Article Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record KnowledgeBaseArticleDeletedEvent(
    Guid ArticleId,
    string Title,
    Guid? CategoryId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
