using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Order Split Completed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderSplitCompletedEvent(Guid OrderSplitId, Guid OriginalOrderId, Guid SplitOrderId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
