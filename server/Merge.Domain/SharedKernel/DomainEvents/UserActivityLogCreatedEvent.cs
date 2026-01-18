using Merge.Domain.Enums;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record UserActivityLogCreatedEvent(
    Guid ActivityLogId,
    Guid? UserId,
    ActivityType ActivityType,
    EntityType EntityType,
    Guid? EntityId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
