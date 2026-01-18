using Merge.Domain.Enums;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ShippingStatusChangedEvent(
    Guid ShippingId,
    Guid OrderId,
    ShippingStatus OldStatus,
    ShippingStatus NewStatus) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

