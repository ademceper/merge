using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PushNotification Sent Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PushNotificationSentEvent(
    Guid PushNotificationId,
    Guid? UserId,
    DateTime SentAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
