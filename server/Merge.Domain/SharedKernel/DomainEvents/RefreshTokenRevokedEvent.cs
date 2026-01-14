using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Refresh Token Revoked Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record RefreshTokenRevokedEvent(
    Guid RefreshTokenId,
    Guid UserId,
    string? RevokedByIp,
    string? ReplacedByTokenHash) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
