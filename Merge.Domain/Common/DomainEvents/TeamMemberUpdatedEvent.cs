using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Team Member Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TeamMemberUpdatedEvent(
    Guid TeamMemberId,
    Guid TeamId,
    Guid UserId,
    string Role) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
