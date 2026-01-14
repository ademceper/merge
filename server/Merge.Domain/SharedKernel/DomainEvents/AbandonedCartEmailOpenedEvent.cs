using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// AbandonedCartEmail Opened Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AbandonedCartEmailOpenedEvent(
    Guid EmailId,
    Guid CartId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
