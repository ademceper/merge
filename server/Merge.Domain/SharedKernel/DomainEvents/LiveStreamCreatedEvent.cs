using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LiveStreamCreatedEvent(
    Guid StreamId,
    Guid SellerId,
    string Title,
    DateTime? ScheduledStartTime) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

