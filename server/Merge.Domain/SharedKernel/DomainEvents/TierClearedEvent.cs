using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TierClearedEvent(
    Guid AccountId,
    Guid UserId,
    Guid OldTierId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
