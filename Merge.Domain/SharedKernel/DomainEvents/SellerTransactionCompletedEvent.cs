using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerTransaction Completed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerTransactionCompletedEvent(
    Guid TransactionId,
    Guid SellerId,
    decimal Amount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
