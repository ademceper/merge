using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// GiftCard Used Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record GiftCardUsedEvent(
    Guid GiftCardId,
    string Code,
    decimal UsedAmount,
    decimal RemainingAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
