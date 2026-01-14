using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Security Alert Acknowledged Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SecurityAlertAcknowledgedEvent(Guid AlertId, Guid AcknowledgedByUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

