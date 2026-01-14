using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Wholesale Price Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record WholesalePriceActivatedEvent(
    Guid WholesalePriceId,
    Guid ProductId,
    Guid? OrganizationId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
