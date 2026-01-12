using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Cart Item Added Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CartItemAddedEvent(Guid CartId, Guid ProductId, int Quantity) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

