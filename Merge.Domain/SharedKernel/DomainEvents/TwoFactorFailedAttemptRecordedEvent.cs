using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Two Factor Authentication Failed Attempt Recorded Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TwoFactorFailedAttemptRecordedEvent(
    Guid TwoFactorAuthId,
    Guid UserId,
    TwoFactorMethod Method,
    int FailedAttempts,
    bool IsLocked) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
