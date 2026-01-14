using Merge.Domain.Enums;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PickPack Status Changed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PickPackStatusChangedEvent(
    Guid PickPackId,
    Guid OrderId,
    PickPackStatus OldStatus,
    PickPackStatus NewStatus) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

