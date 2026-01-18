using Merge.Domain.Modules.Content;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PolicyAcceptanceRevokedEvent(
    Guid AcceptanceId,
    Guid PolicyId,
    Guid UserId,
    string AcceptedVersion) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

