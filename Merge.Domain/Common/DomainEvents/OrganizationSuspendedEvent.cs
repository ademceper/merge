using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Organization Suspended Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrganizationSuspendedEvent(
    Guid OrganizationId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
