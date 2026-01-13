using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// OAuth Provider Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OAuthProviderDeletedEvent(
    Guid ProviderId,
    string Name,
    string ProviderKey) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
