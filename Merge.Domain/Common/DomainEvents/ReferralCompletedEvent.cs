using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Referral Completed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReferralCompletedEvent(
    Guid ReferralId,
    Guid ReferrerId,
    Guid ReferredUserId,
    int PointsAwarded,
    Guid FirstOrderId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
