using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Live Stream Product Added Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveStreamProductAddedEvent(
    Guid StreamId,
    Guid ProductId,
    decimal? SpecialPrice) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

