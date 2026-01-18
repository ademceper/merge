using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SubscriptionPaymentFailedEvent(
    Guid PaymentId,
    Guid UserSubscriptionId,
    decimal Amount,
    string Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
