using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Order Payment Method Changed Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record OrderPaymentMethodChangedEvent(
    Guid OrderId,
    Guid UserId,
    string OldMethod,
    string NewMethod) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
