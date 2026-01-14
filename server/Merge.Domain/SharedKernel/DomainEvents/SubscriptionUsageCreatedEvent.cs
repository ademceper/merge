using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SubscriptionUsage Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
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
