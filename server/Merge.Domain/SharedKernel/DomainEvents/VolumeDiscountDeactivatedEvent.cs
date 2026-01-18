using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record VolumeDiscountDeactivatedEvent(
    Guid VolumeDiscountId,
    Guid ProductId,
    Guid? CategoryId,
    Guid? OrganizationId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
