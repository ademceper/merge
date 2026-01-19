using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record PermissionDeletedEvent(
    Guid PermissionId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
