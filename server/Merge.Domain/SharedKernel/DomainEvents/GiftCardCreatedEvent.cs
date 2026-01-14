using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// GiftCard Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record GiftCardCreatedEvent(
    Guid GiftCardId,
    string Code,
    decimal Amount,
    Guid? PurchasedByUserId,
    Guid? AssignedToUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
