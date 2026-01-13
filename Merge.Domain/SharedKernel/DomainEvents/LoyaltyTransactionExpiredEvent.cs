using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// LoyaltyTransaction Expired Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LoyaltyTransactionExpiredEvent(
    Guid TransactionId,
    Guid UserId,
    int Points) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
