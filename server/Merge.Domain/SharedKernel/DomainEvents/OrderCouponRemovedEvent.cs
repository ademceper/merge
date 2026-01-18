using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record OrderCouponRemovedEvent(
    Guid OrderId,
    Guid UserId,
    Guid? CouponId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
