using Merge.Domain.Modules.Inventory;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PickPack Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PickPackCreatedEvent(
    Guid PickPackId,
    Guid OrderId,
    Guid WarehouseId,
    string PackNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

