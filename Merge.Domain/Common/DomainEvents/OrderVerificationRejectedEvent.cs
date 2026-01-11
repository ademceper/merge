using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Order Verification Rejected Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderVerificationRejectedEvent(
    Guid VerificationId,
    Guid OrderId,
    Guid VerifiedByUserId,
    string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
