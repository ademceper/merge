using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// ETLProcess Started Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ETLProcessStartedEvent(Guid ETLProcessId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
