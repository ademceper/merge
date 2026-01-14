using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

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

