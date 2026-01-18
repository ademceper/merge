using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record FlashSaleProductSaleRecordedEvent(
    Guid FlashSaleProductId,
    Guid FlashSaleId,
    Guid ProductId,
    int Quantity,
    int TotalSoldQuantity,
    int RemainingStock) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
