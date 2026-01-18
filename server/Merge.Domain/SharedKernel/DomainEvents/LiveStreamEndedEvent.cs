using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LiveStreamEndedEvent(
    Guid StreamId,
    Guid SellerId,
    DateTime EndedAt,
    int TotalViewerCount,
    int OrderCount,
    decimal Revenue) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

