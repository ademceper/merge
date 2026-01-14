using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Data Quality Check Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DataQualityCheckDeletedEvent(Guid CheckId, Guid RuleId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
