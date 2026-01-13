using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PreOrder Ready To Ship Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PreOrderReadyToShipEvent(Guid PreOrderId, Guid UserId, DateTime ActualAvailabilityDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
