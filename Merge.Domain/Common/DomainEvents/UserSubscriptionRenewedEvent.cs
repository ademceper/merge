using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// UserSubscription Renewed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserSubscriptionRenewedEvent(
    Guid SubscriptionId,
    Guid UserId,
    DateTime NewEndDate,
    int RenewalCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
