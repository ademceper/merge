using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// DataQualityRule Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DataQualityRuleActivatedEvent(Guid DataQualityRuleId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
