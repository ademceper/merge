using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// SellerProfile Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerProfileActivatedEvent(
    Guid ProfileId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
