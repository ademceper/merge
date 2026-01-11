using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Team Member Removed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TeamMemberRemovedEvent(
    Guid TeamMemberId,
    Guid TeamId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
