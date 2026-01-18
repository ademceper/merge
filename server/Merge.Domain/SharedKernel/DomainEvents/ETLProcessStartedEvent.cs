using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ETLProcessStartedEvent(Guid ETLProcessId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
