using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

// ✅ BOLUM 1.5: Domain Events (ZORUNLU)
public record ProductBundleUpdatedEvent(
    Guid BundleId,
    string Name,
    decimal BundlePrice
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
