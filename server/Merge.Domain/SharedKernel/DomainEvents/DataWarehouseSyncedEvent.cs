using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record DataWarehouseSyncedEvent(Guid DataWarehouseId, DateTime LastSyncAt, int RecordCount, long DataSize) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

