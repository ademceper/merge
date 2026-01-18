using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PaymentCreatedEvent(
    Guid PaymentId,
    Guid OrderId,
    string PaymentMethod,
    string PaymentProvider,
    decimal Amount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
