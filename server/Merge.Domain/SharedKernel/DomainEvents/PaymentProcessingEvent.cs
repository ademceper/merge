using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PaymentProcessingEvent(
    Guid PaymentId,
    Guid OrderId,
    string PaymentMethod,
    string PaymentProvider,
    decimal Amount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
