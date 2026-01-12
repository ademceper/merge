using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// ETLProcess Completed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ETLProcessCompletedEvent(Guid ETLProcessId, int RecordsProcessed, TimeSpan ExecutionTime) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
