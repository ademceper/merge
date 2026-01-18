using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReferralCompletedEvent(
    Guid ReferralId,
    Guid ReferrerId,
    Guid ReferredUserId,
    int PointsAwarded,
    Guid FirstOrderId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
