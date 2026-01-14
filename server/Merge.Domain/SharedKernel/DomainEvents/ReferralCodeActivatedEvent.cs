using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// ReferralCode Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReferralCodeActivatedEvent(
    Guid ReferralCodeId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
