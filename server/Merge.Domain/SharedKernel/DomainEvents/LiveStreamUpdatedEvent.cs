using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Live Stream Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveStreamUpdatedEvent(
    Guid StreamId,
    Guid SellerId,
    string Title,
    DateTime UpdatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
