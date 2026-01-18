using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SubscriptionPaymentRetriedEvent(
    Guid PaymentId,
    Guid UserSubscriptionId,
    int RetryCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
