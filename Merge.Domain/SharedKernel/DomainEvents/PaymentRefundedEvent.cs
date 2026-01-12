using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Payment Refunded Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentRefundedEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal RefundAmount,
    bool IsFullRefund) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
