using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// AbandonedCartEmail Resulted In Purchase Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AbandonedCartEmailResultedInPurchaseEvent(
    Guid EmailId,
    Guid CartId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
