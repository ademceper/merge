using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

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
