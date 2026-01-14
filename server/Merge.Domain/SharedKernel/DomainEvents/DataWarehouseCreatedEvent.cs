using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// DataWarehouse Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DataWarehouseCreatedEvent(Guid DataWarehouseId, string Name, string DataSource) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

