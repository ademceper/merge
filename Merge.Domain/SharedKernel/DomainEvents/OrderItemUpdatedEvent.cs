using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Item Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderItemUpdatedEvent(
    Guid OrderId,
    Guid UserId,
    Guid OrderItemId,
    Guid ProductId,
    int OldQuantity,
    int NewQuantity,
    decimal OldTotalPrice,
    decimal NewTotalPrice) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
