using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SecurityAlertCreatedEvent(Guid AlertId, AlertType AlertType, AlertSeverity Severity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

