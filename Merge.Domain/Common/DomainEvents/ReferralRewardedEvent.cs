using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Referral Rewarded Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReferralRewardedEvent(
    Guid ReferralId,
    Guid ReferrerId,
    int PointsAwarded) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
