using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Payment Fraud Prevention Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentFraudPreventionCreatedEvent(
    Guid CheckId,
    Guid PaymentId,
    PaymentCheckType CheckType,
    int RiskScore,
    VerificationStatus Status) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
