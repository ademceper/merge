using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PreOrder Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PreOrderCreatedEvent(Guid PreOrderId, Guid UserId, Guid ProductId, int Quantity, decimal Price) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

