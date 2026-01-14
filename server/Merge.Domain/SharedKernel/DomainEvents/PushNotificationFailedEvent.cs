using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PushNotification Failed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PushNotificationFailedEvent(
    Guid PushNotificationId,
    Guid? UserId,
    string ErrorMessage) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
