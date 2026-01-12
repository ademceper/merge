using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Live Chat Session Assigned Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveChatSessionAssignedEvent(
    Guid SessionId,
    string SessionIdentifier,
    Guid AgentId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
