using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Live Chat Message Sent Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record LiveChatMessageSentEvent(
    Guid MessageId,
    Guid SessionId,
    string SessionIdentifier,
    Guid? SenderId,
    string SenderType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
