using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Item Added Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderItemAddedEvent(
    Guid OrderId,
    Guid UserId,
    Guid OrderItemId,
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
