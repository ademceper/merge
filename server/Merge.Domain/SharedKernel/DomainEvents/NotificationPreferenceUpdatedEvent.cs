using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// NotificationPreference Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record NotificationPreferenceUpdatedEvent(
    Guid PreferenceId,
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel,
    bool IsEnabled) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
