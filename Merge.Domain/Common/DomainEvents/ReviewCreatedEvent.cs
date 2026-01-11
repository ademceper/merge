using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Review Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReviewCreatedEvent(
    Guid ReviewId,
    Guid UserId,
    Guid ProductId,
    int Rating,
    bool IsVerifiedPurchase) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
