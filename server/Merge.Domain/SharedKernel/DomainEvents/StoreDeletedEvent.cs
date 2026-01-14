using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Store Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record StoreDeletedEvent(
    Guid StoreId,
    Guid SellerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
