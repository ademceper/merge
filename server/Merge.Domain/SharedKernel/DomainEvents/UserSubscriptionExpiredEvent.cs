using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// UserSubscription Expired Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserSubscriptionExpiredEvent(
    Guid UserSubscriptionId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
