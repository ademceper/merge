using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// ReferralCode Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReferralCodeCreatedEvent(
    Guid ReferralCodeId,
    Guid UserId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
