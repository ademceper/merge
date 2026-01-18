using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record FraudAlertCreatedEvent(
    Guid AlertId,
    Guid? UserId,
    FraudAlertType AlertType,
    int RiskScore) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
