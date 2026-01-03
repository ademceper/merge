using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Report Completed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReportCompletedEvent(Guid ReportId, Guid GeneratedBy, string ReportType, DateTime CompletedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

