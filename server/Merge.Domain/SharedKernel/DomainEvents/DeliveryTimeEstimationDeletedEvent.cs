using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// DeliveryTimeEstimation Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DeliveryTimeEstimationDeletedEvent(Guid DeliveryTimeEstimationId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
