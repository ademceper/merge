using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Live Stream Ended Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
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

