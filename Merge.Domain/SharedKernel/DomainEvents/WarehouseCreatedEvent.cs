using Merge.Domain.Modules.Inventory;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

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

