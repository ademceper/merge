using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


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
