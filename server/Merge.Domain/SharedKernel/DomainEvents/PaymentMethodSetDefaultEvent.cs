using Merge.Domain.SharedKernel;
namespace Merge.Domain.SharedKernel.DomainEvents;


public record PaymentMethodSetDefaultEvent(
    Guid PaymentMethodId,
    string Name,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
