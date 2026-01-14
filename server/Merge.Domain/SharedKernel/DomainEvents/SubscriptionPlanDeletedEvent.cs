using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SubscriptionPlan Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SubscriptionPlanDeletedEvent(
    Guid PlanId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
