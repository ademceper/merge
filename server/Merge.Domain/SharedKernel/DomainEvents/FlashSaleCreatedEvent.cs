using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// FlashSale Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FlashSaleCreatedEvent(
    Guid FlashSaleId,
    string Title,
    DateTime StartDate,
    DateTime EndDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
