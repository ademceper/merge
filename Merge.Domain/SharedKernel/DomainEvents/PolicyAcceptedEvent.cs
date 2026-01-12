using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Policy Accepted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PolicyAcceptedEvent(
    Guid AcceptanceId,
    Guid PolicyId,
    Guid UserId,
    string AcceptedVersion) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

