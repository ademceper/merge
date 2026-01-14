using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerApplication Reviewed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerApplicationReviewedEvent(
    Guid ApplicationId,
    Guid UserId,
    Guid ReviewedBy) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
