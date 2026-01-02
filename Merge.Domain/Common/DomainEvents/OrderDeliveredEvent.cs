using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Order Delivered Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderDeliveredEvent(Guid OrderId, Guid UserId, DateTime DeliveredDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

