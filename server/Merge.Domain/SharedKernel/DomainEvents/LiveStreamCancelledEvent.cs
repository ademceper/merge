using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LiveStreamCancelledEvent(
    Guid StreamId,
    Guid SellerId,
    DateTime CancelledAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
