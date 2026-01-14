using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// User Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserDeletedEvent(
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
