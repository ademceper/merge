using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

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
