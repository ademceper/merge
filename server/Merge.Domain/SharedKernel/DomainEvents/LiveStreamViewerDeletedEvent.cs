using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LiveStreamViewerDeletedEvent(
    Guid StreamId,
    Guid ViewerId,
    Guid? UserId,
    string? GuestId,
    DateTime DeletedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
