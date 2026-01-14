using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Payment Fraud Prevention Unblocked Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentFraudPreventionUnblockedEvent(
    Guid CheckId,
    Guid PaymentId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
