using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Live Stream Viewer Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveStreamViewerDeletedEvent(
    Guid StreamId,
    Guid ViewerId,
    Guid? UserId,
    string? GuestId,
    DateTime DeletedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
