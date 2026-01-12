using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Customer Communication Status Changed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CustomerCommunicationStatusChangedEvent(
    Guid CommunicationId,
    string OldStatus,
    string NewStatus) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
