using Merge.Domain.Common;
using Merge.Domain.Enums;

namespace Merge.Domain.Common.DomainEvents;

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
