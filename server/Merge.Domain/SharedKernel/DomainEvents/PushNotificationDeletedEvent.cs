using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PushNotification Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PushNotificationDeletedEvent(
    Guid PushNotificationId,
    Guid? UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
