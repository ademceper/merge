using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record CustomerCommunicationDeletedEvent(
    Guid CommunicationId,
    Guid UserId,
    string CommunicationType,
    string Channel) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
