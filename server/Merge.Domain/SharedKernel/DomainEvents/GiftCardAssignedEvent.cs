using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record GiftCardAssignedEvent(
    Guid GiftCardId,
    string Code,
    Guid AssignedToUserId,
    Guid? PurchasedByUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
