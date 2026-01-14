using Merge.Domain.Modules.Inventory;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PickPack Details Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PickPackDetailsUpdatedEvent(
    Guid PickPackId,
    Guid OrderId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
