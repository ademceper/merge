using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Tier Cleared Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TierClearedEvent(
    Guid AccountId,
    Guid UserId,
    Guid OldTierId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
