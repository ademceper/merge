using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Volume Discount Deactivated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record VolumeDiscountDeactivatedEvent(
    Guid VolumeDiscountId,
    Guid ProductId,
    Guid? CategoryId,
    Guid? OrganizationId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
