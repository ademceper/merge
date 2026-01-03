using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Report Failed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReportFailedEvent(Guid ReportId, Guid GeneratedBy, string ReportType, string ErrorMessage) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

