using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Live Chat Session Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveChatSessionCreatedEvent(
    Guid SessionId,
    string SessionIdentifier,
    Guid? UserId,
    string? GuestName,
    string? Department) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
