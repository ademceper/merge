using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Account Security Event Action Taken Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record AccountSecurityEventActionTakenEvent(
    Guid EventId,
    Guid ActionTakenByUserId,
    string Action) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
