using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerTransaction Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerTransactionCreatedEvent(
    Guid TransactionId,
    Guid SellerId,
    string TransactionType,
    decimal Amount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
