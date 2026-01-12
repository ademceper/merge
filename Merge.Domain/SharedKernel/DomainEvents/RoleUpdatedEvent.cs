using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Role Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record RoleUpdatedEvent(
    Guid RoleId,
    string RoleName,
    string? Description) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
