using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TwoFactorBackupCodeRemovedEvent(
    Guid TwoFactorAuthId,
    Guid UserId,
    TwoFactorMethod Method,
    int RemainingBackupCodeCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
