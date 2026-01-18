using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TierUpdatedEvent(
    Guid AccountId,
    Guid UserId,
    Guid? OldTierId,
    Guid NewTierId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
