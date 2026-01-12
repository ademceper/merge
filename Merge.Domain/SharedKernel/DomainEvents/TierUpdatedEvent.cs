using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Tier Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TierUpdatedEvent(
    Guid AccountId,
    Guid UserId,
    Guid? OldTierId,
    Guid NewTierId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
