using Merge.Domain.Enums;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PickPackStatusChangedEvent(
    Guid PickPackId,
    Guid OrderId,
    PickPackStatus OldStatus,
    PickPackStatus NewStatus) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

