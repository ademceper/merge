using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// FlashSaleProduct Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FlashSaleProductDeletedEvent(
    Guid FlashSaleProductId,
    Guid FlashSaleId,
    Guid ProductId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
