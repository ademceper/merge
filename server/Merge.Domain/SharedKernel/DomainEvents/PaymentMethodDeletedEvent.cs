using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PaymentMethod Deleted Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentMethodDeletedEvent(
    Guid PaymentMethodId,
    string Name,
    string Code) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
