using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// CommissionPayout Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record CommissionPayoutCreatedEvent(
    Guid PayoutId,
    Guid SellerId,
    decimal NetAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
