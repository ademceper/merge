using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record CouponUsageCreatedEvent(
    Guid CouponUsageId,
    Guid CouponId,
    Guid UserId,
    Guid OrderId,
    decimal DiscountAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
