using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Wholesale Price Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record WholesalePriceUpdatedEvent(
    Guid WholesalePriceId,
    Guid ProductId,
    Guid? OrganizationId,
    decimal Price) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
