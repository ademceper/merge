using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record LiveChatMessageSentEvent(
    Guid MessageId,
    Guid SessionId,
    string SessionIdentifier,
    Guid? SenderId,
    string SenderType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
