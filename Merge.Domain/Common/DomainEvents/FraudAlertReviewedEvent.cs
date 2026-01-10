using Merge.Domain.Common;
using Merge.Domain.Enums;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Fraud Alert Reviewed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FraudAlertReviewedEvent(
    Guid AlertId,
    Guid ReviewedByUserId,
    FraudAlertStatus Status) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
