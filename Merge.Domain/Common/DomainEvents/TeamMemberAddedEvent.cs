using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Team Member Added Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TeamMemberAddedEvent(
    Guid TeamMemberId,
    Guid TeamId,
    Guid UserId,
    string Role) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
