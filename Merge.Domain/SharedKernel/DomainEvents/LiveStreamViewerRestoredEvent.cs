using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Live Stream Viewer Restored Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record LiveStreamViewerRestoredEvent(
    Guid StreamId,
    Guid ViewerId,
    Guid? UserId,
    string? GuestId,
    DateTime RestoredAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
