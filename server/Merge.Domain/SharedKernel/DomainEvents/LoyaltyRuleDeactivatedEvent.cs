using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// LoyaltyRule Deactivated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LoyaltyRuleDeactivatedEvent(
    Guid RuleId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
