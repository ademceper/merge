using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Gift Card Discount Applied Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderGiftCardDiscountAppliedEvent(
    Guid OrderId,
    Guid UserId,
    decimal DiscountAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
