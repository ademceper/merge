using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SellerCommissionPaidEvent(
    Guid CommissionId,
    Guid SellerId,
    decimal NetAmount,
    string PaymentReference) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
