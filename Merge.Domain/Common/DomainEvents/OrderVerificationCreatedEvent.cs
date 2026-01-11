using Merge.Domain.Common;
using Merge.Domain.Enums;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Order Verification Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderVerificationCreatedEvent(
    Guid VerificationId,
    Guid OrderId,
    VerificationType VerificationType,
    int RiskScore,
    bool RequiresManualReview) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
