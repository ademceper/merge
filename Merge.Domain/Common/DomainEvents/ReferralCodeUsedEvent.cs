using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// ReferralCode Used Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReferralCodeUsedEvent(
    Guid ReferralCodeId,
    Guid UserId,
    string Code,
    int UsageCount,
    int MaxUsage) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
