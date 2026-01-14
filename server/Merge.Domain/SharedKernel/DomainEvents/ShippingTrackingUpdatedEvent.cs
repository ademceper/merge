using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Shipping Tracking Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ShippingTrackingUpdatedEvent(
    Guid ShippingId,
    Guid OrderId,
    string TrackingNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
