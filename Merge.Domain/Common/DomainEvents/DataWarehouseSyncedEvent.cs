using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// DataWarehouse Synced Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DataWarehouseSyncedEvent(Guid DataWarehouseId, DateTime LastSyncAt, int RecordCount, long DataSize) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

