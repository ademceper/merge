using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

public record ProductBundleCreatedEvent(
    Guid BundleId,
    string Name,
    decimal BundlePrice,
    decimal DiscountPercentage
) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
