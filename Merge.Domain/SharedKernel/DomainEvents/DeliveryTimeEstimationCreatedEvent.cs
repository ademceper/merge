using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// DeliveryTimeEstimation Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DeliveryTimeEstimationCreatedEvent(
    Guid DeliveryTimeEstimationId,
    Guid? ProductId,
    Guid? CategoryId,
    Guid? WarehouseId,
    int MinDays,
    int MaxDays,
    int AverageDays) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
