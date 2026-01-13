using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Put On Hold Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderPutOnHoldEvent(Guid OrderId, Guid UserId, string? Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
