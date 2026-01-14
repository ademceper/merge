using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Security Alert Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SecurityAlertCreatedEvent(Guid AlertId, AlertType AlertType, AlertSeverity Severity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

