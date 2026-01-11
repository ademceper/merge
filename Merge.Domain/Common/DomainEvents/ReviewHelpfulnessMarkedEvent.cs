using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Review Helpfulness Marked Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record ReviewHelpfulnessMarkedEvent(
    Guid ReviewId,
    Guid UserId,
    bool IsHelpful) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
