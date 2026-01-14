using Merge.Domain.Modules.Inventory;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Inventory Created Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record InventoryCreatedEvent(
    Guid InventoryId,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

