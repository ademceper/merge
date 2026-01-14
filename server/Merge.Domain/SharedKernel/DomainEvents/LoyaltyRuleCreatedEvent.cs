using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// LoyaltyRule Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LoyaltyRuleCreatedEvent(
    Guid RuleId,
    string Name,
    LoyaltyTransactionType Type,
    int PointsAwarded) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
