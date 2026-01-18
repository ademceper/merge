using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record AccountSecurityEventActionTakenEvent(
    Guid EventId,
    Guid ActionTakenByUserId,
    string Action) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
