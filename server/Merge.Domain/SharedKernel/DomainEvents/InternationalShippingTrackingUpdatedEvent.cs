using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record InternationalShippingTrackingUpdatedEvent(
    Guid InternationalShippingId,
    Guid OrderId,
    string TrackingNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
