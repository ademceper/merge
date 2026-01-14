using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// ReferralCode Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReferralCodeDeletedEvent(
    Guid ReferralCodeId,
    Guid UserId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
