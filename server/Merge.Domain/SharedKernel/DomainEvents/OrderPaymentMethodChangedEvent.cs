using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record OrderPaymentMethodChangedEvent(
    Guid OrderId,
    Guid UserId,
    string OldMethod,
    string NewMethod) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
