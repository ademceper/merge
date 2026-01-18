using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record OAuthAccountUpdatedEvent(
    Guid OAuthAccountId,
    Guid UserId,
    string Provider) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
