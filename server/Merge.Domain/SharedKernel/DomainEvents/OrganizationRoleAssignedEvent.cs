using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record OrganizationRoleAssignedEvent(
    Guid OrganizationRoleId,
    Guid OrganizationId,
    Guid UserId,
    Guid RoleId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
