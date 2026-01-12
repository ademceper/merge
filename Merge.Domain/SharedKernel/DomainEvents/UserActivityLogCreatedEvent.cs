using Merge.Domain.Enums;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// UserActivityLog Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserActivityLogCreatedEvent(
    Guid ActivityLogId,
    Guid? UserId,
    string ActivityType,
    string EntityType,
    Guid? EntityId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
