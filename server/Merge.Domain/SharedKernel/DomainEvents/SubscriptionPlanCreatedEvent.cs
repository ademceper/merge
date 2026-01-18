using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SubscriptionPlanCreatedEvent(
    Guid PlanId,
    string Name,
    SubscriptionPlanType PlanType,
    decimal Price,
    string Currency) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
