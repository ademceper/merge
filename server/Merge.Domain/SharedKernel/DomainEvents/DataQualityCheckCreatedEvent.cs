using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;

namespace Merge.Domain.SharedKernel.DomainEvents;


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
