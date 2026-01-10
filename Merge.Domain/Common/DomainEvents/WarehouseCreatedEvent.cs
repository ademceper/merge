using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Warehouse Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record WarehouseCreatedEvent(
    Guid WarehouseId,
    string Name,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

