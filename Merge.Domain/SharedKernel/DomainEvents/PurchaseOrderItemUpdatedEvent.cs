using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Purchase Order Item Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PurchaseOrderItemUpdatedEvent(
    Guid PurchaseOrderId,
    Guid ProductId,
    Guid PurchaseOrderItemId,
    int OldQuantity,
    int NewQuantity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
