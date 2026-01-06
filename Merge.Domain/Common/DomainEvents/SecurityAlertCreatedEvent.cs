using Merge.Domain.Common;
using Merge.Domain.Enums;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Security Alert Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SecurityAlertCreatedEvent(Guid AlertId, string AlertType, AlertSeverity Severity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

