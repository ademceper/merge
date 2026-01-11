using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// SellerApplication Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerApplicationCreatedEvent(
    Guid ApplicationId,
    Guid UserId,
    string BusinessName) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
