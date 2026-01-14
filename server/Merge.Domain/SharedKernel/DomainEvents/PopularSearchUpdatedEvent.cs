using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Popular Search Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PopularSearchUpdatedEvent(
    Guid PopularSearchId,
    string SearchTerm,
    int SearchCount,
    int ClickThroughCount,
    decimal ClickThroughRate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
