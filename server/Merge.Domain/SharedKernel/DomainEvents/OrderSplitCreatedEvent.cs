using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record OrderSplitCreatedEvent(Guid OrderSplitId, Guid OriginalOrderId, Guid SplitOrderId, string SplitReason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
