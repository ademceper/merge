using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Shipping Details Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ShippingDetailsUpdatedEvent(
    Guid ShippingId,
    Guid OrderId,
    string UpdateType) : IDomainEvent // UpdateType: "TrackingNumber", "EstimatedDeliveryDate", "ShippingLabelUrl", "ShippingCost"
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
