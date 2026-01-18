using Merge.Domain.Enums;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReportCompletedEvent(Guid ReportId, Guid GeneratedBy, string ReportType, DateTime CompletedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

