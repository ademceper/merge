using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Cart Item Removed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CartItemRemovedEvent(Guid CartId, Guid ProductId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

