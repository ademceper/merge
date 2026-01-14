using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Verification Verified Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderVerificationVerifiedEvent(
    Guid VerificationId,
    Guid OrderId,
    Guid VerifiedByUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
