using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record RefreshTokenCreatedEvent(
    Guid RefreshTokenId,
    Guid UserId,
    DateTime ExpiresAt) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
