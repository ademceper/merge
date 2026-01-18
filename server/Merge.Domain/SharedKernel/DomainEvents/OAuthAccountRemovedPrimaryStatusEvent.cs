using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record OAuthAccountRemovedPrimaryStatusEvent(
    Guid OAuthAccountId,
    Guid UserId,
    string Provider) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
