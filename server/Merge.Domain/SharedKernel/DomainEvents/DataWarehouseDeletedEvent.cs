using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// DataWarehouse Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DataWarehouseDeletedEvent(Guid DataWarehouseId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

