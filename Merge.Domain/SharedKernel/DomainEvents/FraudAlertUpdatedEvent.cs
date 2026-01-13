using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// FraudAlert Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FraudAlertUpdatedEvent(
    Guid FraudAlertId,
    Guid? UserId,
    FraudAlertType AlertType,
    int RiskScore,
    FraudAlertStatus Status) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
