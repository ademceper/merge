using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TwoFactorAuthCreatedEvent(
    Guid TwoFactorAuthId,
    Guid UserId,
    TwoFactorMethod Method) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
