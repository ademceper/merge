using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


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
