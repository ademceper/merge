using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Store Suspended Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record StoreSuspendedEvent(
    Guid StoreId,
    Guid SellerId,
    string? Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
