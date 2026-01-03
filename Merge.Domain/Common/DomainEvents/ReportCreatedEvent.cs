using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Report Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReportCreatedEvent(Guid ReportId, Guid GeneratedBy, string ReportType, DateTime StartDate, DateTime EndDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

