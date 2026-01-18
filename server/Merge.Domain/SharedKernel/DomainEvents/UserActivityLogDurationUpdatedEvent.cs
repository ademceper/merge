using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record UserActivityLogDurationUpdatedEvent(
    Guid ActivityLogId,
    Guid? UserId,
    int DurationMs) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
