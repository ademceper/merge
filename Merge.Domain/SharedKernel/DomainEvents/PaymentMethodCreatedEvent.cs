using Merge.Domain.Modules.Payment;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PaymentMethod Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record PaymentMethodCreatedEvent(
    Guid PaymentMethodId,
    string Name,
    string Code,
    bool IsActive,
    bool IsDefault) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
