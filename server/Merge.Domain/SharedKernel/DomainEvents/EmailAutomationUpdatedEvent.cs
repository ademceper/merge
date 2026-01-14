using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// EmailAutomation Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailAutomationUpdatedEvent(
    Guid AutomationId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
