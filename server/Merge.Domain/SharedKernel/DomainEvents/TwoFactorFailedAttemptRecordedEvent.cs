using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TwoFactorFailedAttemptRecordedEvent(
    Guid TwoFactorAuthId,
    Guid UserId,
    TwoFactorMethod Method,
    int FailedAttempts,
    bool IsLocked) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
