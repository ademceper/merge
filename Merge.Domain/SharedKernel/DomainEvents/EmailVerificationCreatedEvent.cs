using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// EmailVerification Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record EmailVerificationCreatedEvent(
    Guid VerificationId,
    Guid UserId,
    string Email) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
