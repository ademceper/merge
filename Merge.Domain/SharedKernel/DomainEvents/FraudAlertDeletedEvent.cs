using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// FraudAlert Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FraudAlertDeletedEvent(
    Guid FraudAlertId,
    Guid? UserId,
    FraudAlertType AlertType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
