using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PurchaseOrderItemRemovedEvent(
    Guid PurchaseOrderId,
    Guid ProductId,
    Guid PurchaseOrderItemId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
