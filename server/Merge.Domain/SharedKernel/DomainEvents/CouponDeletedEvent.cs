using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Coupon Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CouponDeletedEvent(
    Guid CouponId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
