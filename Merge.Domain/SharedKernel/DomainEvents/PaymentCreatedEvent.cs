using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Payment Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentCreatedEvent(
    Guid PaymentId,
    Guid OrderId,
    string PaymentMethod,
    string PaymentProvider,
    decimal Amount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
