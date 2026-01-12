using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// GiftCard Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record GiftCardActivatedEvent(
    Guid GiftCardId,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
