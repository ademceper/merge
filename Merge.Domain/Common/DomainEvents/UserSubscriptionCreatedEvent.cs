using Merge.Domain.Common;
using Merge.Domain.Enums;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// UserSubscription Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserSubscriptionCreatedEvent(
    Guid SubscriptionId,
    Guid UserId,
    Guid PlanId,
    SubscriptionStatus Status,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
