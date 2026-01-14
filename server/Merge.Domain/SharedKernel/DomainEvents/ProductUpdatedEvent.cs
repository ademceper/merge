using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Product Updated Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record ProductUpdatedEvent(
    Guid ProductId,
    string Name,
    string SKU,
    Guid CategoryId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

