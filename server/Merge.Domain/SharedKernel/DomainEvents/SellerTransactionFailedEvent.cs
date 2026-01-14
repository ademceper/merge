using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerTransaction Failed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerTransactionFailedEvent(
    Guid TransactionId,
    Guid SellerId,
    decimal Amount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
