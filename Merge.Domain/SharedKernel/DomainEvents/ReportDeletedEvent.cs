using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Report Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReportDeletedEvent(Guid ReportId, Guid GeneratedBy, string ReportType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
