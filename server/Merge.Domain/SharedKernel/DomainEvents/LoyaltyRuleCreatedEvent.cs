using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LoyaltyRuleCreatedEvent(
    Guid RuleId,
    string Name,
    LoyaltyTransactionType Type,
    int PointsAwarded) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
