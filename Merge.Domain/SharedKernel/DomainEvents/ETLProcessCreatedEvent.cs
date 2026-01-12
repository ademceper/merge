using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// ETLProcess Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ETLProcessCreatedEvent(Guid ETLProcessId, string Name, string ProcessType, string SourceSystem, string TargetSystem) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
