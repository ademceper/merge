using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Report Schedule Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReportScheduleCreatedEvent(Guid ScheduleId, Guid OwnerId, string ReportType, string Frequency) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

