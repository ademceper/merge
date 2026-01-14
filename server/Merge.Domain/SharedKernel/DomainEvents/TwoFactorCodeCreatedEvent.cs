using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Two Factor Code Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TwoFactorCodeCreatedEvent(
    Guid CodeId,
    Guid UserId,
    TwoFactorMethod Method,
    TwoFactorPurpose Purpose) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
