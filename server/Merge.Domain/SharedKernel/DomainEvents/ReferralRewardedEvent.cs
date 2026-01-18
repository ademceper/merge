using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReferralRewardedEvent(
    Guid ReferralId,
    Guid ReferrerId,
    int PointsAwarded) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
