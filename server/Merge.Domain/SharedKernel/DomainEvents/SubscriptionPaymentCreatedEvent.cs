using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SubscriptionPaymentCreatedEvent(
    Guid SubscriptionPaymentId,
    Guid UserSubscriptionId,
    decimal Amount,
    DateTime BillingPeriodStart,
    DateTime BillingPeriodEnd) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
