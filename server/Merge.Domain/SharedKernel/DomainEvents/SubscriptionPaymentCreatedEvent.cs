using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SubscriptionPayment Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SubscriptionPaymentCreatedEvent(
    Guid SubscriptionPaymentId,
    Guid UserSubscriptionId,
    decimal Amount,
    DateTime BillingPeriodStart,
    DateTime BillingPeriodEnd) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
