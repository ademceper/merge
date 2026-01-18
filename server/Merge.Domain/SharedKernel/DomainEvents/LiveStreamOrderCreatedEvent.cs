using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LiveStreamOrderCreatedEvent(
    Guid StreamId,
    Guid OrderId,
    Guid? ProductId,
    decimal OrderAmount,
    DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
