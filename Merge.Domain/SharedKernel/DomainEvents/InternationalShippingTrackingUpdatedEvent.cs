using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// InternationalShipping Tracking Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record InternationalShippingTrackingUpdatedEvent(
    Guid InternationalShippingId,
    Guid OrderId,
    string TrackingNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
