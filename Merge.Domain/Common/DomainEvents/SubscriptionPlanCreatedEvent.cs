using Merge.Domain.Common;
using Merge.Domain.Enums;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// SubscriptionPlan Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SubscriptionPlanCreatedEvent(
    Guid PlanId,
    string Name,
    SubscriptionPlanType PlanType,
    decimal Price,
    string Currency) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
