using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

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
