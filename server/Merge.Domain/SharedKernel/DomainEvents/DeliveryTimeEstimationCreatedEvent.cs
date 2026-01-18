using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


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
