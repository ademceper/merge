using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

// ✅ BOLUM 1.5: Domain Events (ZORUNLU)
public record SizeGuideDeletedEvent(
    Guid SizeGuideId,
    string Name
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
