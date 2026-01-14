using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerCommission Reverted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerCommissionRevertedEvent(
    Guid CommissionId,
    Guid SellerId,
    decimal NetAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
