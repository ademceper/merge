using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// InternationalShipping Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record InternationalShippingDeletedEvent(
    Guid InternationalShippingId,
    Guid OrderId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
