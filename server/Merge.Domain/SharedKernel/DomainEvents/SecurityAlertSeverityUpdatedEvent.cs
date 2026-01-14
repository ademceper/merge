using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Security Alert Severity Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SecurityAlertSeverityUpdatedEvent(
    Guid AlertId,
    AlertSeverity NewSeverity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
