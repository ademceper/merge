using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Security Alert Resolved Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SecurityAlertResolvedEvent(Guid AlertId, Guid ResolvedByUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

