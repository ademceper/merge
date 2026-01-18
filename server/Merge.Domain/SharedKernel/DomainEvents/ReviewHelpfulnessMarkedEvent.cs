using Merge.Domain.Modules.Catalog;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record ReviewHelpfulnessMarkedEvent(
    Guid ReviewId,
    Guid UserId,
    bool IsHelpful) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
