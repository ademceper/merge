using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SubscriptionUsageCreatedEvent(
    Guid SubscriptionUsageId,
    Guid UserSubscriptionId,
    Guid UserId,
    string Feature,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int? Limit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
