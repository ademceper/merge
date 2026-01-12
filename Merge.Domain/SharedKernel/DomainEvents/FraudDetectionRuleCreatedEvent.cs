using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Fraud Detection Rule Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FraudDetectionRuleCreatedEvent(
    Guid RuleId,
    string Name,
    FraudRuleType RuleType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
