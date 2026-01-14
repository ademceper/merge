using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Purchase Order Item Removed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PurchaseOrderItemRemovedEvent(
    Guid PurchaseOrderId,
    Guid ProductId,
    Guid PurchaseOrderItemId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
