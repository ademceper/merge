using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReportDeletedEvent(Guid ReportId, Guid GeneratedBy, string ReportType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
