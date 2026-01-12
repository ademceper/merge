using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Report Schedule Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReportScheduleActivatedEvent(Guid ScheduleId, Guid OwnerId, DateTime? NextRunAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

