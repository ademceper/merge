using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Live Chat Session Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveChatSessionDeletedEvent(
    Guid SessionId,
    string SessionIdentifier,
    Guid? UserId,
    Guid? AgentId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
