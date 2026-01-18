using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record GiftCardCreatedEvent(
    Guid GiftCardId,
    string Code,
    decimal Amount,
    Guid? PurchasedByUserId,
    Guid? AssignedToUserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
