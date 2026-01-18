using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record WholesalePriceUpdatedEvent(
    Guid WholesalePriceId,
    Guid ProductId,
    Guid? OrganizationId,
    decimal Price) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
