using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record OrderGiftCardDiscountAppliedEvent(
    Guid OrderId,
    Guid UserId,
    decimal DiscountAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
