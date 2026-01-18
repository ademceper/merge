using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LiveChatSessionDeletedEvent(
    Guid SessionId,
    string SessionIdentifier,
    Guid? UserId,
    Guid? AgentId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
