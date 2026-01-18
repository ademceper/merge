using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record CustomerCommunicationCreatedEvent(
    Guid CommunicationId,
    Guid UserId,
    string CommunicationType,
    string Channel,
    string Direction) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
