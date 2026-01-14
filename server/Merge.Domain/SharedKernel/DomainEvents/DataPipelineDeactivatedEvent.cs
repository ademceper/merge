using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// DataPipeline Deactivated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DataPipelineDeactivatedEvent(Guid DataPipelineId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
