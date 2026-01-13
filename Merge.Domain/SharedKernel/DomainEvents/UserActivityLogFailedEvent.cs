using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// UserActivityLog Failed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record UserActivityLogFailedEvent(
    Guid ActivityLogId,
    Guid? UserId,
    string ErrorMessage) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
