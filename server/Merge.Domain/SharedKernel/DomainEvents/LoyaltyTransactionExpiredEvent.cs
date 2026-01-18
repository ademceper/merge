using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LoyaltyTransactionExpiredEvent(
    Guid TransactionId,
    Guid UserId,
    int Points) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
