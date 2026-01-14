using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// UserActivityLog Duration Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserActivityLogDurationUpdatedEvent(
    Guid ActivityLogId,
    Guid? UserId,
    int DurationMs) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
