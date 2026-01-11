using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

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
