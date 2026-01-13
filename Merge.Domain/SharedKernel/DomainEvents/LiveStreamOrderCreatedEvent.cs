using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Live Stream Order Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveStreamOrderCreatedEvent(
    Guid StreamId,
    Guid OrderId,
    Guid? ProductId,
    decimal OrderAmount,
    DateTime CreatedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
