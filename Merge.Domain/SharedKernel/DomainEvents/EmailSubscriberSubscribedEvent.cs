using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// EmailSubscriber Subscribed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailSubscriberSubscribedEvent(
    Guid SubscriberId,
    string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
