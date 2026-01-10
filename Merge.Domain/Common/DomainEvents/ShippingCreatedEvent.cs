using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Shipping Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ShippingCreatedEvent(
    Guid ShippingId,
    Guid OrderId,
    string ShippingProvider,
    decimal ShippingCost) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

