using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record InternationalShippingCreatedEvent(
    Guid InternationalShippingId,
    Guid OrderId,
    string OriginCountry,
    string DestinationCountry,
    string ShippingMethod,
    decimal ShippingCost) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
