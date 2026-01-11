using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Customer Communication Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CustomerCommunicationCreatedEvent(
    Guid CommunicationId,
    Guid UserId,
    string CommunicationType,
    string Channel,
    string Direction) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
