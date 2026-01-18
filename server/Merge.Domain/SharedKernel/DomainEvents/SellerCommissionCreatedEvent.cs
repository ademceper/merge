using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SellerCommissionCreatedEvent(
    Guid CommissionId,
    Guid SellerId,
    Guid OrderId,
    decimal CommissionAmount,
    decimal NetAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
