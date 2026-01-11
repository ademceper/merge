using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Organization Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrganizationUpdatedEvent(
    Guid OrganizationId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
