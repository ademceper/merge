using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record RoleCreatedEvent(
    Guid RoleId,
    string Name,
    string? Description) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
