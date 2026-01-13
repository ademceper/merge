using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Refresh Token Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record RefreshTokenCreatedEvent(
    Guid RefreshTokenId,
    Guid UserId,
    DateTime ExpiresAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
