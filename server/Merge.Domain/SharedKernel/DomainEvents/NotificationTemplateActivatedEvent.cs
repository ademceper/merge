using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// NotificationTemplate Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record NotificationTemplateActivatedEvent(
    Guid TemplateId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
