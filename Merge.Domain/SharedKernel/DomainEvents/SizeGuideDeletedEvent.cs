using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

// ✅ BOLUM 1.5: Domain Events (ZORUNLU)
public record SizeGuideDeletedEvent(
    Guid SizeGuideId,
    string Name
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
