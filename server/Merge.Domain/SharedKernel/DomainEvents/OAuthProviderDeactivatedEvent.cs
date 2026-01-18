using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record OAuthProviderDeactivatedEvent(
    Guid ProviderId,
    string Name,
    string ProviderKey) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
