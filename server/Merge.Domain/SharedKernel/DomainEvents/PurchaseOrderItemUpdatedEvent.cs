using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PurchaseOrderItemUpdatedEvent(
    Guid PurchaseOrderId,
    Guid ProductId,
    Guid PurchaseOrderItemId,
    int OldQuantity,
    int NewQuantity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
