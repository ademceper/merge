using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Product Created Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    string SKU,
    Guid CategoryId,
    Guid? SellerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

