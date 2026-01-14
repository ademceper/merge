using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// GiftCard Assigned Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record GiftCardAssignedEvent(
    Guid GiftCardId,
    string Code,
    Guid AssignedToUserId,
    Guid? PurchasedByUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
