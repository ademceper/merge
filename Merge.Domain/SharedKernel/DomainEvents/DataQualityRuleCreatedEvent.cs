using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// DataQualityRule Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DataQualityRuleCreatedEvent(
    Guid DataQualityRuleId,
    string Name,
    string RuleType,
    string TargetEntity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
