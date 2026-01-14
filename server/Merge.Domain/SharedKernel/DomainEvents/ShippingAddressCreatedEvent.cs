using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// ShippingAddress Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ShippingAddressCreatedEvent(
    Guid ShippingAddressId,
    Guid UserId,
    string Label,
    string City,
    string Country,
    bool IsDefault) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
