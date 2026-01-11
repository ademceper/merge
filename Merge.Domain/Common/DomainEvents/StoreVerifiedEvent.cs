using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Store Verified Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record StoreVerifiedEvent(
    Guid StoreId,
    Guid SellerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
