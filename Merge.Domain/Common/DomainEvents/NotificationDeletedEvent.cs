using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Notification Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record NotificationDeletedEvent(
    Guid NotificationId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
