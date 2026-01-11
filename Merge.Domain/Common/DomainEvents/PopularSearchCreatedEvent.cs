using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Popular Search Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PopularSearchCreatedEvent(
    Guid PopularSearchId,
    string SearchTerm) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
