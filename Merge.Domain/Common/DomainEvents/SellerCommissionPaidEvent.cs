using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// SellerCommission Paid Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerCommissionPaidEvent(
    Guid CommissionId,
    Guid SellerId,
    decimal NetAmount,
    string PaymentReference) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
