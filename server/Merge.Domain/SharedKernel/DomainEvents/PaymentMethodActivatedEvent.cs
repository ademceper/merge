using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PaymentMethod Activated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentMethodActivatedEvent(
    Guid PaymentMethodId,
    string Name,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
