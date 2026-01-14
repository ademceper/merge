using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Live Stream Started Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveStreamStartedEvent(
    Guid StreamId,
    Guid SellerId,
    DateTime StartedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

