using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Payment Processing Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentProcessingEvent(
    Guid PaymentId,
    Guid OrderId,
    string PaymentMethod,
    string PaymentProvider,
    decimal Amount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
