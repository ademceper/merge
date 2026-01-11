using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Order Split Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderSplitCreatedEvent(Guid OrderSplitId, Guid OriginalOrderId, Guid SplitOrderId, string SplitReason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
