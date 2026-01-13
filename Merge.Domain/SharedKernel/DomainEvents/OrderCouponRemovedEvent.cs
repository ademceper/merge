using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Coupon Removed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderCouponRemovedEvent(
    Guid OrderId,
    Guid UserId,
    Guid? CouponId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
