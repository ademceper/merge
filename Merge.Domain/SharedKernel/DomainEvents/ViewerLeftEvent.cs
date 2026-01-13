using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Viewer Left Live Stream Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
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
