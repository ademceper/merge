using Merge.Domain.Common;
using Merge.Domain.Enums;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Account Security Event Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AccountSecurityEventCreatedEvent(
    Guid EventId,
    Guid UserId,
    SecurityEventType EventType,
    SecurityEventSeverity Severity,
    bool IsSuspicious,
    bool RequiresAction) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
