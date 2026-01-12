using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerProfile Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerProfileActivatedEvent(
    Guid ProfileId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
