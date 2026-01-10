using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// EmailSubscriber Unsubscribed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailSubscriberUnsubscribedEvent(
    Guid SubscriberId,
    string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
