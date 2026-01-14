using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Two Factor Authentication Backup Code Removed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TwoFactorBackupCodeRemovedEvent(
    Guid TwoFactorAuthId,
    Guid UserId,
    TwoFactorMethod Method,
    int RemainingBackupCodeCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
