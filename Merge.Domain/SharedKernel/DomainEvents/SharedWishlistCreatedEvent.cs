using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Shared Wishlist Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SharedWishlistCreatedEvent(
    Guid SharedWishlistId,
    Guid UserId,
    string Name,
    bool IsPublic) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
