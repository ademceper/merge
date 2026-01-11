using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

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
