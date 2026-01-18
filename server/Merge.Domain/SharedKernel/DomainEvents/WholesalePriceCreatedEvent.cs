using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record WholesalePriceCreatedEvent(
    Guid WholesalePriceId,
    Guid ProductId,
    Guid? OrganizationId,
    int MinQuantity,
    int? MaxQuantity,
    decimal Price) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
