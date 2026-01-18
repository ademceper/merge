using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record FraudDetectionRuleCreatedEvent(
    Guid RuleId,
    string Name,
    FraudRuleType RuleType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
