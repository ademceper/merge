using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Two Factor Authentication Disabled Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TwoFactorDisabledEvent(
    Guid UserId,
    Guid TwoFactorAuthId,
    TwoFactorMethod Method) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

