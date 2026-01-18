using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record DataPipelineDeletedEvent(Guid DataPipelineId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
