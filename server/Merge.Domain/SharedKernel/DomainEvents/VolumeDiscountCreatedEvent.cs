using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Volume Discount Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record VolumeDiscountCreatedEvent(
    Guid VolumeDiscountId,
    Guid ProductId,
    Guid? CategoryId,
    Guid? OrganizationId,
    int MinQuantity,
    int? MaxQuantity,
    decimal DiscountPercentage,
    decimal? FixedDiscountAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
