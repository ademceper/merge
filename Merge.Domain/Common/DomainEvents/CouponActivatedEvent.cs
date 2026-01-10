using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Coupon Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CouponActivatedEvent(
    Guid CouponId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
