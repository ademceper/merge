using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record OrganizationRoleRemovedEvent(
    Guid OrganizationRoleId,
    Guid OrganizationId,
    Guid UserId,
    Guid RoleId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
