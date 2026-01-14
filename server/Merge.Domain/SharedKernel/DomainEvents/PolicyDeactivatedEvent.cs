using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Policy Deactivated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PolicyDeactivatedEvent(
    Guid PolicyId,
    string PolicyType,
    string Version) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

