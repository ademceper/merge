using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record AbandonedCartEmailResultedInPurchaseEvent(
    Guid EmailId,
    Guid CartId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
