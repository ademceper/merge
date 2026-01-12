using Merge.Domain.Enums;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Report Completed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReportCompletedEvent(Guid ReportId, Guid GeneratedBy, string ReportType, DateTime CompletedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

