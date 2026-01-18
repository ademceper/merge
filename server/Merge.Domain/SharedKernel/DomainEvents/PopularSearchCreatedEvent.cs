using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PopularSearchCreatedEvent(
    Guid PopularSearchId,
    string SearchTerm) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
