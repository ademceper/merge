using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// FAQ Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FaqDeletedEvent(
    Guid FaqId,
    string Question,
    string Category) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
