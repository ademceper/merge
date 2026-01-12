using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// DataWarehouse Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DataWarehouseActivatedEvent(Guid DataWarehouseId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

