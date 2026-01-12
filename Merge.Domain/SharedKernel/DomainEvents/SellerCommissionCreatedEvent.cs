using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerCommission Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerCommissionCreatedEvent(
    Guid CommissionId,
    Guid SellerId,
    Guid OrderId,
    decimal CommissionAmount,
    decimal NetAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
