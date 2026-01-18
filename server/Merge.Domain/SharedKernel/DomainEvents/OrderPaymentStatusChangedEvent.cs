using Merge.Domain.Modules.Ordering;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record OrderPaymentStatusChangedEvent(
    Guid OrderId,
    Guid UserId,
    PaymentStatus OldStatus,
    PaymentStatus NewStatus) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
