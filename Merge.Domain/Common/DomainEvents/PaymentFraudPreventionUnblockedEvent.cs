using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Payment Fraud Prevention Unblocked Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentFraudPreventionUnblockedEvent(
    Guid CheckId,
    Guid PaymentId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
