using Merge.Domain.Common;
using Merge.Domain.Enums;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Fraud Alert Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FraudAlertCreatedEvent(
    Guid AlertId,
    Guid? UserId,
    FraudAlertType AlertType,
    int RiskScore) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
