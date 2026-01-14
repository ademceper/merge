using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// CommissionPayout Processed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CommissionPayoutProcessedEvent(
    Guid PayoutId,
    Guid SellerId,
    decimal NetAmount,
    string TransactionReference) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
