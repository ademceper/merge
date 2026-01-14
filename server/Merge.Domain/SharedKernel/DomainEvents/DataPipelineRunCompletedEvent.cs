using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// DataPipeline Run Completed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DataPipelineRunCompletedEvent(Guid DataPipelineId, int RecordsProcessed, int RecordsFailed, string? ErrorMessage) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
