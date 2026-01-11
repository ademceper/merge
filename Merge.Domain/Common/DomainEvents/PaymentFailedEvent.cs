using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Payment Failed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentFailedEvent(
    Guid PaymentId,
    Guid OrderId,
    string FailureReason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
