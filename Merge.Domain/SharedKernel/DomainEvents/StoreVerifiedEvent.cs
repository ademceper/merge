using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Store Verified Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record StoreVerifiedEvent(
    Guid StoreId,
    Guid SellerId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
