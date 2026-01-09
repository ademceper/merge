using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Live Stream Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveStreamCreatedEvent(
    Guid StreamId,
    Guid SellerId,
    string Title,
    DateTime? ScheduledStartTime) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

