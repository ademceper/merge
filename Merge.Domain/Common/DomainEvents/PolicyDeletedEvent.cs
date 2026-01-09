using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Policy Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PolicyDeletedEvent(
    Guid PolicyId,
    string PolicyType,
    string Version) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

