using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Two Factor Authentication Enabled Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TwoFactorEnabledEvent(
    Guid UserId,
    Guid TwoFactorAuthId,
    TwoFactorMethod Method) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

