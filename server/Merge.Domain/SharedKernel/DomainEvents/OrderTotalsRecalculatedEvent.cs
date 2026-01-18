using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record OrderTotalsRecalculatedEvent(
    Guid OrderId,
    Guid UserId,
    decimal SubTotal,
    decimal ShippingCost,
    decimal Tax,
    decimal? CouponDiscount,
    decimal? GiftCardDiscount,
    decimal TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
