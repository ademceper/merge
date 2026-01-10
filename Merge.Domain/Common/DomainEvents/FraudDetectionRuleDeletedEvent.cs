using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Fraud Detection Rule Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FraudDetectionRuleDeletedEvent(
    Guid RuleId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
