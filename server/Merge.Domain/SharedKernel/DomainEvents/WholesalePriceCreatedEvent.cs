using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Wholesale Price Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
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
