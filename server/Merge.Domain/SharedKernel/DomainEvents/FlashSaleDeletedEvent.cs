using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// FlashSale Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FlashSaleDeletedEvent(
    Guid FlashSaleId,
    string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
