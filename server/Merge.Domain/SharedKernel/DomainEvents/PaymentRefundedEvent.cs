using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PaymentRefundedEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal RefundAmount,
    bool IsFullRefund) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
