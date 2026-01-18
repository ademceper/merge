using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record PaymentMethodUnsetDefaultEvent(
    Guid PaymentMethodId,
    string Name,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
