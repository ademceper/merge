using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record EmailVerifiedEvent(
    Guid UserId,
    string Email,
    Guid EmailVerificationId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

