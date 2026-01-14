using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Notification Read Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record NotificationReadEvent(
    Guid NotificationId,
    Guid UserId,
    DateTime ReadAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
