using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerProfile Verified Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerProfileVerifiedEvent(
    Guid ProfileId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
