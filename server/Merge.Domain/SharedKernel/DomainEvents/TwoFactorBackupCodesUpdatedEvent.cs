using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Two Factor Authentication Backup Codes Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TwoFactorBackupCodesUpdatedEvent(
    Guid TwoFactorAuthId,
    Guid UserId,
    TwoFactorMethod Method,
    int BackupCodeCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
