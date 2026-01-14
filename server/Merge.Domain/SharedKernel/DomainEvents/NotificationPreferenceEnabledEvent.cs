using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// NotificationPreference Enabled Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record NotificationPreferenceEnabledEvent(
    Guid PreferenceId,
    Guid UserId,
    NotificationType NotificationType,
    NotificationChannel Channel) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
