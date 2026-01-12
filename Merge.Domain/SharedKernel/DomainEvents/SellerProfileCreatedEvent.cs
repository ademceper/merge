using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerProfile Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerProfileCreatedEvent(
    Guid ProfileId,
    Guid UserId,
    string StoreName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
