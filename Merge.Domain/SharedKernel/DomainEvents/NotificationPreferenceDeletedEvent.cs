using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// NotificationPreference Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record NotificationPreferenceDeletedEvent(
    Guid PreferenceId,
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
