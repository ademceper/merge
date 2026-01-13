using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Refunded Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderRefundedEvent(Guid OrderId, Guid UserId, decimal RefundAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
