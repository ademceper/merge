using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Email Verified Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailVerifiedEvent(
    Guid UserId,
    string Email,
    Guid EmailVerificationId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

