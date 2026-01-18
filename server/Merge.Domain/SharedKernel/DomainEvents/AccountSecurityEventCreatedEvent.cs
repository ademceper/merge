using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


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
