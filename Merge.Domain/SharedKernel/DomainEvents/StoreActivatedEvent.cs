using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Store Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record StoreActivatedEvent(
    Guid StoreId,
    Guid SellerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
