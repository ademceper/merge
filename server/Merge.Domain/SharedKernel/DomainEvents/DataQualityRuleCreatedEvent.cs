using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record DataQualityRuleCreatedEvent(
    Guid DataQualityRuleId,
    string Name,
    string RuleType,
    string TargetEntity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
