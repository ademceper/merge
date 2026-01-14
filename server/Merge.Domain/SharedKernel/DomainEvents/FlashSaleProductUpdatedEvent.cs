using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// FlashSaleProduct Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FlashSaleProductUpdatedEvent(
    Guid FlashSaleProductId,
    Guid FlashSaleId,
    Guid ProductId,
    string UpdateType) : IDomainEvent // UpdateType: "SalePrice", "StockLimit", "SortOrder"
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
