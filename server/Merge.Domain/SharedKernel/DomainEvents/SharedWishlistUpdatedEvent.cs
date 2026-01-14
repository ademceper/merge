using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Shared Wishlist Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SharedWishlistUpdatedEvent(
    Guid SharedWishlistId,
    Guid UserId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
