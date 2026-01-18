using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LiveChatSessionAssignedEvent(
    Guid SessionId,
    string SessionIdentifier,
    Guid AgentId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
