using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record RoleUpdatedEvent(
    Guid RoleId,
    string RoleName,
    string? Description) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
