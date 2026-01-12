using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// User Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserActivatedEvent(
    Guid UserId,
    string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
