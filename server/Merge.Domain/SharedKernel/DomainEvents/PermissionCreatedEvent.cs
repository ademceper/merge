using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record PermissionCreatedEvent(
    Guid PermissionId,
    string Name,
    string Category,
    string Resource,
    string Action) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
