using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderCreatedEvent(Guid OrderId, Guid UserId, decimal TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

