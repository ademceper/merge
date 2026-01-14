using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// EmailSubscriber Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailSubscriberCreatedEvent(
    Guid SubscriberId,
    string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
