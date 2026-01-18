using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ShippingCreatedEvent(
    Guid ShippingId,
    Guid OrderId,
    string ShippingProvider,
    decimal ShippingCost) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

