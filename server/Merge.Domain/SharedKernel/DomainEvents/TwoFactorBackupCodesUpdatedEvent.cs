using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TwoFactorBackupCodesUpdatedEvent(
    Guid TwoFactorAuthId,
    Guid UserId,
    TwoFactorMethod Method,
    int BackupCodeCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
