using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Coupon Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CouponActivatedEvent(
    Guid CouponId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
