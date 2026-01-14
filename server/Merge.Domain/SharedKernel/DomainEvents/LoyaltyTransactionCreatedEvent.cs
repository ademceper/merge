using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// LoyaltyTransaction Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LoyaltyTransactionCreatedEvent(
    Guid TransactionId,
    Guid UserId,
    Guid LoyaltyAccountId,
    int Points,
    LoyaltyTransactionType Type) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
