using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ViewerLeftEvent(
    Guid StreamId,
    Guid ViewerId,
    Guid? UserId,
    string? GuestId,
    DateTime LeftAt,
    int WatchDurationInSeconds) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
