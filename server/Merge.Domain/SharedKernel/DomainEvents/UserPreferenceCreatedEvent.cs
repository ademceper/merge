using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// UserPreference Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserPreferenceCreatedEvent(
    Guid UserPreferenceId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
