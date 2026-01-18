using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SharedWishlistCreatedEvent(
    Guid SharedWishlistId,
    Guid UserId,
    string Name,
    bool IsPublic) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
