using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Notification Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record NotificationCreatedEvent(
    Guid NotificationId,
    Guid UserId,
    NotificationType Type,
    string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
