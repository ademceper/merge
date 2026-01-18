using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record ProductComparisonCreatedEvent(
    Guid ComparisonId,
    Guid UserId,
    string? Name,
    int ProductCount
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
