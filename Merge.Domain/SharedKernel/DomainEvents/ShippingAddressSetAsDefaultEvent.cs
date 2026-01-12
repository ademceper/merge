using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// ShippingAddress Set As Default Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ShippingAddressSetAsDefaultEvent(
    Guid ShippingAddressId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
