using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record AccountSecurityEventSeverityUpdatedEvent(
    Guid EventId,
    Guid UserId,
    SecurityEventType EventType,
    SecurityEventSeverity NewSeverity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
