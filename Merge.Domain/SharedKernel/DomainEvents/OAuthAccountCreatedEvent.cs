using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// OAuth Account Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OAuthAccountCreatedEvent(
    Guid OAuthAccountId,
    Guid UserId,
    string Provider,
    string ProviderUserId,
    bool IsPrimary) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
