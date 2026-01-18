using Merge.Domain.Modules.Analytics;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record DataWarehouseCreatedEvent(Guid DataWarehouseId, string Name, string DataSource) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

