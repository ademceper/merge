using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Product Deleted Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record ProductDeletedEvent(
    Guid ProductId,
    string Name,
    string SKU) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

