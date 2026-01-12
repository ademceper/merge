using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Payment Completed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentCompletedEvent(
    Guid PaymentId,
    Guid OrderId,
    string TransactionId,
    string? PaymentReference,
    decimal Amount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
