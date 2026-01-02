using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Order Shipped Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderShippedEvent(Guid OrderId, Guid UserId, DateTime ShippedDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

