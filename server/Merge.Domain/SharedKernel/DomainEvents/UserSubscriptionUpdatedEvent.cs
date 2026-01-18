using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record UserSubscriptionUpdatedEvent(
    Guid SubscriptionId,
    Guid UserId,
    bool? AutoRenewChanged,
    bool? PaymentMethodChanged) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
