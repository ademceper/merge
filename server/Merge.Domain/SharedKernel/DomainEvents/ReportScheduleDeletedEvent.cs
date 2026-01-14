using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Report Schedule Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReportScheduleDeletedEvent(Guid ScheduleId, Guid OwnerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
