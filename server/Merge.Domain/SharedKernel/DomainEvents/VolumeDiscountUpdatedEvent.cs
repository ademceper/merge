using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Volume Discount Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record VolumeDiscountUpdatedEvent(
    Guid VolumeDiscountId,
    Guid ProductId,
    Guid? CategoryId,
    Guid? OrganizationId,
    decimal DiscountPercentage,
    decimal? FixedDiscountAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
