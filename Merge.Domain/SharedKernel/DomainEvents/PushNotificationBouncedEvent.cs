using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PushNotification Bounced Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PushNotificationBouncedEvent(
    Guid PushNotificationId,
    Guid? UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
