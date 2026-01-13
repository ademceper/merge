using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PushNotification Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PushNotificationCreatedEvent(
    Guid PushNotificationId,
    Guid? UserId,
    Guid? DeviceId,
    NotificationType NotificationType,
    string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
