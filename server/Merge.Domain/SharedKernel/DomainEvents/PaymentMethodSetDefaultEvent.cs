using Merge.Domain.SharedKernel;
namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PaymentMethodSetDefaultEvent - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentMethodSetDefaultEvent(
    Guid PaymentMethodId,
    string Name,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
