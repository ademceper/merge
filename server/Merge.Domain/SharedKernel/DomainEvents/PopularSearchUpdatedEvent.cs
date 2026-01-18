using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PopularSearchUpdatedEvent(
    Guid PopularSearchId,
    string SearchTerm,
    int SearchCount,
    int ClickThroughCount,
    decimal ClickThroughRate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
