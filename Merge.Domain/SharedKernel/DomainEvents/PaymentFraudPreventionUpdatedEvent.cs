using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PaymentFraudPrevention Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentFraudPreventionUpdatedEvent(
    Guid PaymentFraudPreventionId,
    Guid PaymentId,
    PaymentCheckType CheckType,
    int RiskScore,
    VerificationStatus Status) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
