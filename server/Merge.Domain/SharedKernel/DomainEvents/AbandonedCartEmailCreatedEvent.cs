using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record AbandonedCartEmailCreatedEvent(
    Guid EmailId,
    Guid CartId,
    Guid UserId,
    AbandonedCartEmailType EmailType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
