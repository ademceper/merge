using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record ProductComparisonUpdatedEvent(
    Guid ComparisonId,
    Guid UserId,
    int ProductCount
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
