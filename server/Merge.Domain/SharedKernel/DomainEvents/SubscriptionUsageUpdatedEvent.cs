using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SubscriptionUsageUpdatedEvent(
    Guid SubscriptionUsageId,
    Guid UserSubscriptionId,
    Guid UserId,
    string Feature,
    int UsageCount,
    int? Limit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
