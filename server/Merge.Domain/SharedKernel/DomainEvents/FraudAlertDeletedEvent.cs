using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record FraudAlertDeletedEvent(
    Guid FraudAlertId,
    Guid? UserId,
    FraudAlertType AlertType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
