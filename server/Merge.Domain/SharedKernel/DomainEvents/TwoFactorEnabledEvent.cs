using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TwoFactorEnabledEvent(
    Guid UserId,
    Guid TwoFactorAuthId,
    TwoFactorMethod Method) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

