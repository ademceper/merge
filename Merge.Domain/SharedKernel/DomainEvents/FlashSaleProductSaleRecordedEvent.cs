using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// FlashSaleProduct Sale Recorded Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
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
