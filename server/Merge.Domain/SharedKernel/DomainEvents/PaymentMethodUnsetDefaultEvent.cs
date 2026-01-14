using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PaymentMethod Unset Default Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentMethodUnsetDefaultEvent(
    Guid PaymentMethodId,
    string Name,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
