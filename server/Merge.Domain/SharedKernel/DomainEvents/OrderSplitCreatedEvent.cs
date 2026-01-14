using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Split Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderSplitCreatedEvent(Guid OrderSplitId, Guid OriginalOrderId, Guid SplitOrderId, string SplitReason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
