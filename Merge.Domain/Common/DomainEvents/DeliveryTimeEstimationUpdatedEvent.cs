using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// DeliveryTimeEstimation Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record DeliveryTimeEstimationUpdatedEvent(
    Guid DeliveryTimeEstimationId,
    int MinDays,
    int MaxDays,
    int AverageDays) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
