using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Search Click Recorded Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SearchClickRecordedEvent(
    Guid SearchHistoryId,
    Guid ProductId,
    Guid? UserId,
    string SearchTerm) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
