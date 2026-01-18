using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record UserSubscriptionRenewedEvent(
    Guid SubscriptionId,
    Guid UserId,
    DateTime NewEndDate,
    int RenewalCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
