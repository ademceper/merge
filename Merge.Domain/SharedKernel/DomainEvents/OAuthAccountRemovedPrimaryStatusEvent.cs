using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// OAuth Account Removed Primary Status Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OAuthAccountRemovedPrimaryStatusEvent(
    Guid OAuthAccountId,
    Guid UserId,
    string Provider) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
