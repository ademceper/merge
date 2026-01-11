using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Knowledge Base Category Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record KnowledgeBaseCategoryCreatedEvent(
    Guid CategoryId,
    string Name,
    string Slug,
    Guid? ParentCategoryId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
