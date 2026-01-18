using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record NotificationPreferenceCreatedEvent(
    Guid PreferenceId,
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel,
    bool IsEnabled) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
