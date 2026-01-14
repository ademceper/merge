using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// NotificationTemplate Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record NotificationTemplateCreatedEvent(
    Guid TemplateId,
    string Name,
    NotificationType Type) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
