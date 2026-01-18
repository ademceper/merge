using Merge.Domain.Enums;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record StockMovementCreatedEvent(
    Guid StockMovementId,
    Guid ProductId,
    Guid WarehouseId,
    StockMovementType MovementType,
    int Quantity,
    int QuantityBefore,
    int QuantityAfter) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
