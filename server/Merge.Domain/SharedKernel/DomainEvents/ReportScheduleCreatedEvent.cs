using Merge.Domain.Enums;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReportScheduleCreatedEvent(Guid ScheduleId, Guid OwnerId, string ReportType, string Frequency) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

