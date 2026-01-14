using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

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
