using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Referral Expired Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReferralExpiredEvent(
    Guid ReferralId,
    Guid ReferrerId,
    Guid ReferredUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
