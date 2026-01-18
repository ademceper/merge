using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SellerApplicationReviewedEvent(
    Guid ApplicationId,
    Guid UserId,
    Guid ReviewedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
