using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// SellerApplication Submitted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerApplicationSubmittedEvent(
    Guid ApplicationId,
    Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
