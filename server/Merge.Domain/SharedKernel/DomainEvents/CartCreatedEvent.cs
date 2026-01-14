using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Cart Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CartCreatedEvent(Guid CartId, Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

