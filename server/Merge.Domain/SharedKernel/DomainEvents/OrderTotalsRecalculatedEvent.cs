using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Totals Recalculated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
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
