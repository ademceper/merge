using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Account Security Event Severity Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AccountSecurityEventSeverityUpdatedEvent(
    Guid EventId,
    Guid UserId,
    SecurityEventType EventType,
    SecurityEventSeverity NewSeverity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
