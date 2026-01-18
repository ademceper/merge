using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SecurityAlertSeverityUpdatedEvent(
    Guid AlertId,
    AlertSeverity NewSeverity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
