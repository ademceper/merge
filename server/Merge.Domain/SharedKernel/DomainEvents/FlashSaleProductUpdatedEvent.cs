using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record FlashSaleProductUpdatedEvent(
    Guid FlashSaleProductId,
    Guid FlashSaleId,
    Guid ProductId,
    string UpdateType) : IDomainEvent // UpdateType: "SalePrice", "StockLimit", "SortOrder"
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
