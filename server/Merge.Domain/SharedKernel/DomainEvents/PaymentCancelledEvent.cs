using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Payment Cancelled Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentCancelledEvent(
    Guid PaymentId,
    Guid OrderId,
    string? Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
