using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record CommissionPayoutProcessedEvent(
    Guid PayoutId,
    Guid SellerId,
    decimal NetAmount,
    string TransactionReference) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
