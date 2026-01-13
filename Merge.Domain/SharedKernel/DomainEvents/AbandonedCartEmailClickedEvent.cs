using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// AbandonedCartEmail Clicked Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AbandonedCartEmailClickedEvent(
    Guid EmailId,
    Guid CartId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
