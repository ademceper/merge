using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// InternationalShipping In Customs Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record InternationalShippingInCustomsEvent(
    Guid InternationalShippingId,
    Guid OrderId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
