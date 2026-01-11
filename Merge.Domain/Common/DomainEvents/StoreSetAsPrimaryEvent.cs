using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Store Set As Primary Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record StoreSetAsPrimaryEvent(
    Guid StoreId,
    Guid SellerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
