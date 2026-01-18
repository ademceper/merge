using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record ProductBundleUpdatedEvent(
    Guid BundleId,
    string Name,
    decimal BundlePrice
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
