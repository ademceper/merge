using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// User Deactivated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserDeactivatedEvent(
    Guid UserId,
    string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
