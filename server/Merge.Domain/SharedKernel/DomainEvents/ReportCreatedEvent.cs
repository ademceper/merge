using Merge.Domain.Enums;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReportCreatedEvent(Guid ReportId, Guid GeneratedBy, string ReportType, DateTime StartDate, DateTime EndDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

