using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// UserSubscription Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserSubscriptionActivatedEvent(
    Guid SubscriptionId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
