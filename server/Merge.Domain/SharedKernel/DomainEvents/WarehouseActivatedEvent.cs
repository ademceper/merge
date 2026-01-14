using Merge.Domain.Modules.Inventory;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Warehouse Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record WarehouseActivatedEvent(Guid WarehouseId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

