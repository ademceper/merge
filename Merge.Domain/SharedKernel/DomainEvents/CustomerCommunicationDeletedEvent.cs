using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Customer Communication Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CustomerCommunicationDeletedEvent(
    Guid CommunicationId,
    Guid UserId,
    string CommunicationType,
    string Channel) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
