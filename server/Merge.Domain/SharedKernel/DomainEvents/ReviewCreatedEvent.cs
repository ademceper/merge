using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

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
