using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

// ✅ BOLUM 1.5: Domain Events (ZORUNLU)
public record ProductComparisonUpdatedEvent(
    Guid ComparisonId,
    Guid UserId,
    int ProductCount
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
