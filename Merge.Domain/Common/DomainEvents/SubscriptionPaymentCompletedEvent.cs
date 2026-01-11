using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// SubscriptionPayment Completed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SubscriptionPaymentCompletedEvent(
    Guid PaymentId,
    Guid UserSubscriptionId,
    decimal Amount,
    string TransactionId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
