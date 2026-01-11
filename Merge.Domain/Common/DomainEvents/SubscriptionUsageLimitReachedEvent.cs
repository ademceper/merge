using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// SubscriptionUsage Limit Reached Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SubscriptionUsageLimitReachedEvent(
    Guid UsageId,
    Guid UserSubscriptionId,
    Guid UserId,
    string Feature,
    int UsageCount,
    int Limit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
