using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// AbandonedCartEmail Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AbandonedCartEmailCreatedEvent(
    Guid EmailId,
    Guid CartId,
    Guid UserId,
    AbandonedCartEmailType EmailType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
