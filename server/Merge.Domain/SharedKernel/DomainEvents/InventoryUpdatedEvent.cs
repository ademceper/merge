using Merge.Domain.Modules.Inventory;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Inventory Updated Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record InventoryUpdatedEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

