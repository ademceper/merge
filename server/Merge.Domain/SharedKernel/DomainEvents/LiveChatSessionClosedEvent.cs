using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LiveChatSessionClosedEvent(
    Guid SessionId,
    string SessionIdentifier,
    Guid? UserId,
    DateTime ClosedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
