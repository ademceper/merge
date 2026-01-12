using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// DataQualityCheck Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DataQualityCheckCreatedEvent(
    Guid DataQualityCheckId,
    Guid RuleId,
    int RecordsChecked,
    int RecordsPassed,
    int RecordsFailed,
    DataQualityCheckStatus Status) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
