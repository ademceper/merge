using Merge.Domain.Modules.Inventory;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Warehouse Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record WarehouseUpdatedEvent(
    Guid WarehouseId,
    string Name,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
