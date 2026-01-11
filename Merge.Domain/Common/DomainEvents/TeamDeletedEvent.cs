using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Team Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TeamDeletedEvent(
    Guid TeamId,
    Guid OrganizationId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
