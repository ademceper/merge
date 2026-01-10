using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Coupon Used Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CouponUsedEvent(
    Guid CouponId,
    string Code,
    int UsedCount,
    int UsageLimit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
