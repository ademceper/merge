using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Payment Fraud Prevention Blocked Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentFraudPreventionBlockedEvent(
    Guid CheckId,
    Guid PaymentId,
    string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
