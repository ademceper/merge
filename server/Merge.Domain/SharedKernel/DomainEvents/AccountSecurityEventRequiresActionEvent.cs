using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record AccountSecurityEventRequiresActionEvent(
    Guid EventId,
    Guid UserId,
    SecurityEventType EventType,
    SecurityEventSeverity Severity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
