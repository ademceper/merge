using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Account Security Event Marked As Suspicious Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AccountSecurityEventMarkedAsSuspiciousEvent(
    Guid EventId,
    Guid UserId,
    SecurityEventType EventType,
    SecurityEventSeverity Severity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
