using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Coupon Applied Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderCouponAppliedEvent(
    Guid OrderId,
    Guid UserId,
    Guid CouponId,
    decimal DiscountAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
