using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReferralCreatedEvent(
    Guid ReferralId,
    Guid ReferrerId,
    Guid ReferredUserId,
    string ReferralCode) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
