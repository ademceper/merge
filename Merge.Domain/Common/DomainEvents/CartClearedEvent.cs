using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Cart Cleared Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CartClearedEvent(Guid CartId, Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

