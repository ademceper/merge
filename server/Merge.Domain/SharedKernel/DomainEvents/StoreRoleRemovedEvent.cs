using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record StoreRoleRemovedEvent(
    Guid StoreRoleId,
    Guid StoreId,
    Guid UserId,
    Guid RoleId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
