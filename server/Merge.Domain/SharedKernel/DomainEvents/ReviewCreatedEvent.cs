using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReviewCreatedEvent(
    Guid ReviewId,
    Guid UserId,
    Guid ProductId,
    int Rating,
    bool IsVerifiedPurchase) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
