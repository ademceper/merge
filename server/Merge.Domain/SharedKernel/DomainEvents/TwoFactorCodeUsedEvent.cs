using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TwoFactorCodeUsedEvent(
    Guid CodeId,
    Guid UserId,
    TwoFactorMethod Method,
    TwoFactorPurpose Purpose) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
