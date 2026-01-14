using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PreOrder Cancelled Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PreOrderCancelledEvent(Guid PreOrderId, Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

