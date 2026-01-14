using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// EmailAutomation Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailAutomationCreatedEvent(
    Guid AutomationId,
    string Name,
    EmailAutomationType Type) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
