using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// KnowledgeBaseCategory Updated Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record KnowledgeBaseCategoryUpdatedEvent(
    Guid CategoryId,
    string Name,
    string Slug) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
