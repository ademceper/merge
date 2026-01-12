using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// GiftCard Redeemed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record GiftCardRedeemedEvent(
    Guid GiftCardId,
    string Code,
    Guid? AssignedToUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
