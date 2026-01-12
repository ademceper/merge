using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Report Schedule Deactivated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReportScheduleDeactivatedEvent(Guid ScheduleId, Guid OwnerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

