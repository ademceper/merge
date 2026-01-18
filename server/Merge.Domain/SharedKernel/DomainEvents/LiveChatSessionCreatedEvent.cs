using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LiveChatSessionCreatedEvent(
    Guid SessionId,
    string SessionIdentifier,
    Guid? UserId,
    string? GuestName,
    string? Department) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
