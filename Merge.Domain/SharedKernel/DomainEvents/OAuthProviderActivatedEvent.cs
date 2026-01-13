using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// OAuth Provider Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OAuthProviderActivatedEvent(
    Guid ProviderId,
    string Name,
    string ProviderKey) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
