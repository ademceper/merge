using Merge.Domain.Enums;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Report Failed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReportFailedEvent(Guid ReportId, Guid GeneratedBy, string ReportType, string ErrorMessage) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

