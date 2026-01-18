using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


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
