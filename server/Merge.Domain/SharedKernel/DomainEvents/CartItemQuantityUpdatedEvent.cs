using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Cart Item Quantity Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CartItemQuantityUpdatedEvent(
    Guid CartId,
    Guid CartItemId,
    Guid ProductId,
    int OldQuantity,
    int NewQuantity
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
