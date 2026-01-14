using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Two Factor Auth Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TwoFactorAuthCreatedEvent(
    Guid TwoFactorAuthId,
    Guid UserId,
    TwoFactorMethod Method) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
