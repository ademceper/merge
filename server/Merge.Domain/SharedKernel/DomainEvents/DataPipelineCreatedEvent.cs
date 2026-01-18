using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record DataPipelineCreatedEvent(Guid DataPipelineId, string Name, string SourceType, string TargetType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
