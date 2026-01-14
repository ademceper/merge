using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Live Stream Cancelled Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveStreamCancelledEvent(
    Guid StreamId,
    Guid SellerId,
    DateTime CancelledAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
