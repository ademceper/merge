using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// FlashSaleProduct Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FlashSaleProductCreatedEvent(
    Guid FlashSaleProductId,
    Guid FlashSaleId,
    Guid ProductId,
    decimal SalePrice,
    int StockLimit) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
