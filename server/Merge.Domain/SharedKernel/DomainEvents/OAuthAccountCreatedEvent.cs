using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record OAuthAccountCreatedEvent(
    Guid OAuthAccountId,
    Guid UserId,
    string Provider,
    string ProviderUserId,
    bool IsPrimary) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
