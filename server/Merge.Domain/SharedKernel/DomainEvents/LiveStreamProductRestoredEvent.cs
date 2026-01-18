using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LiveStreamProductRestoredEvent(
    Guid StreamId,
    Guid ProductId,
    DateTime RestoredAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
