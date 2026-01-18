using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PurchaseOrderItemAddedEvent(
    Guid PurchaseOrderId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
