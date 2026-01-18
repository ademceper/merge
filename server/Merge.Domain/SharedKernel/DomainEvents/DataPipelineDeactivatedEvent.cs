using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record DataPipelineDeactivatedEvent(Guid DataPipelineId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
