using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Viewer Joined Live Stream Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ViewerJoinedEvent(
    Guid StreamId,
    Guid ViewerId,
    Guid? UserId,
    string? GuestId,
    DateTime JoinedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
