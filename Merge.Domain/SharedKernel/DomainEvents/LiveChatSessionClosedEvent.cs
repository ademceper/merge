using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Live Chat Session Closed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveChatSessionClosedEvent(
    Guid SessionId,
    string SessionIdentifier,
    Guid? UserId,
    DateTime ClosedAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
