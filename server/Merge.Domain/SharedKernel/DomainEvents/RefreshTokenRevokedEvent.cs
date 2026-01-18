using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record RefreshTokenRevokedEvent(
    Guid RefreshTokenId,
    Guid UserId,
    string? RevokedByIp,
    string? ReplacedByTokenHash) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
