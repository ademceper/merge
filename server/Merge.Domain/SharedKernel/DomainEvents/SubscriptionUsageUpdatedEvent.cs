using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SubscriptionUsage Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
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
