using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Coupon Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CouponCreatedEvent(
    Guid CouponId,
    string Code,
    decimal DiscountAmount,
    decimal? DiscountPercentage) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
