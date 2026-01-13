using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// InternationalShipping Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
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
