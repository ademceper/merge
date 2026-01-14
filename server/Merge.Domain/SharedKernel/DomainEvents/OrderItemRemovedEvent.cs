using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Item Removed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderItemRemovedEvent(
    Guid OrderId,
    Guid UserId,
    Guid OrderItemId,
    Guid ProductId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
