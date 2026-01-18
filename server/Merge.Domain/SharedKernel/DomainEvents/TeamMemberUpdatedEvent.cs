using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TeamMemberUpdatedEvent(
    Guid TeamMemberId,
    Guid TeamId,
    Guid UserId,
    string Role) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
